using BDMS.Shared;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BDMS.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> GetDashboardById(int user_id, CancellationToken ct)
        {
            try
            {
                var totalDonors = await _context.Donors.CountAsync(x => x.DeletedAt == null, ct);
                var totalDonations = await _context.Donations.CountAsync(x => x.DeletedAt == null && x.Status == "completed", ct);
                var totalBloodRequests = await _context.BloodRequests.CountAsync(x => x.DeletedAt == null, ct);

                var bloodStock = await _context.BloodInventories
                    .Where(x => x.DeletedAt == null && x.Status == "available")
                    .GroupBy(x => x.BloodGroup)
                    .Select(g => new
                    {
                        bloodGroup = g.Key,
                        units = g.Sum(x => x.Units)
                    })
                    .ToListAsync(ct);

                var last6Months = Enumerable.Range(0, 6)
                    .Select(i => DateTime.UtcNow.AddMonths(-i))
                    .Select(d => new { Year = d.Year, Month = d.Month, Name = d.ToString("MMM") })
                    .Reverse()
                    .ToList();

                var donationsByMonth = await _context.Donations
                    .Where(x => x.DeletedAt == null)
                    .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync(ct);

                var requestsByMonth = await _context.BloodRequests
                    .Where(x => x.DeletedAt == null)
                    .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync(ct);

                var monthlyTrends = last6Months.Select(m => new
                {
                    month = m.Name,
                    donations = donationsByMonth.FirstOrDefault(d => d.Year == m.Year && d.Month == m.Month)?.Count ?? 0,
                    requests = requestsByMonth.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Count ?? 0
                }).ToList();

                var dashboardData = new
                {
                    totalDonors,
                    totalDonations,
                    totalBloodRequests,
                    bloodStock,
                    monthlyTrends
                };

                var jsonString = JsonSerializer.Serialize(dashboardData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return Result<string>.Success(jsonString);
            }
            catch (Exception ex)
            {
                return Result<string>.SystemError("Failed to load dashboard", ex.Message);
            }
        }
    }
}
