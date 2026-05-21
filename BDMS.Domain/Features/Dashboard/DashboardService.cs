using BDMS.Shared;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BDMS.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public DashboardService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<Result<string>> GetDashboardById(int user_id, CancellationToken ct)
        {
            var dbProvider = _config["DatabaseProvider"] ?? "MSSQL";

            if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
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
                    return Result<string>.SystemError("Failed to load in-memory dashboard", ex.Message);
                }
            }

            using IDbConnection db = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var command = new CommandDefinition("sp_GetDashboardData",
                                                new { user_id = user_id },
                                                commandType: CommandType.StoredProcedure,
                                                cancellationToken : ct);

            try
            {
                var result = await db.QueryFirstOrDefaultAsync<string>(command);

                return Result<string>.Success(result ?? "{}");
            }
            catch (Exception ex) 
            {
                return Result<string>.SystemError("Failed to load dashboard", ex.Message);
            }
        }
    }
}
