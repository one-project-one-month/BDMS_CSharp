using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory
{
    internal class BloodInventoryService : IBloodInventoryService
    {
        private readonly AppDbContext _db;
        private const int BloodExpiryDays = 42;

        public BloodInventoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<BloodInventoryResModel>> AddtoInventory(int donationId, CancellationToken cancellationToken)
        {
            try
            {
                var donation = await _db.Donations.FirstOrDefaultAsync(d => d.Id == donationId && d.DeletedAt == null, cancellationToken);

                if (donation == null)
                    return Result<BloodInventoryResModel>.NotFound("Donation not found");

                if (!string.Equals(donation.Status, "completed", StringComparison.OrdinalIgnoreCase))
                    return Result<BloodInventoryResModel>.ValidationError("Only completed donations can be added to inventory.");

                var exists = await _db.BloodInventories.AnyAsync(bi => bi.DonationId == donationId && bi.DeletedAt == null, cancellationToken);

                if (exists)
                return Result<BloodInventoryResModel>.ValidationError(
                    "Inventory record already exists for this donation.");

                var collectedDate = donation.DonationDate ?? DateOnly.FromDateTime(DateTime.Now);

                var inventory = new Database.AppDbContextModels.BloodInventory
                {
                    DonationId = donationId,
                    HospitalId = donation.HospitalId,
                    BloodGroup = donation.BloodGroup,
                    Units = donation.UnitsDonated ?? 1,
                    CollectedAt = collectedDate,
                    ExpiredAt = collectedDate.AddDays(BloodExpiryDays),
                    Status = EnumBloodInventoryStatus.Available.ToDatabaseValue()
                };

                await _db.BloodInventories.AddAsync(inventory, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);


                return Result<BloodInventoryResModel>.Success(ToResponse(inventory),
                    "Blood added to inventory successfully.");
            }
            catch (Exception ex)
            {
                return Result<BloodInventoryResModel>.SystemError(
                 $"Error adding to inventory: {ex.Message}");
            }
        }

        public async Task<Result<string>> Delete(int id, CancellationToken ct)
        {
            try
            {
                var item = await _db.BloodInventories.FirstOrDefaultAsync(bi => bi.Id == id && bi.DeletedAt == null, ct);
                if(item == null)
                {
                    return Result<string>.ValidationError("Inventory Not Found");
                }
                item.UpdatedAt = DateTime.UtcNow;
                item.DeletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
                return Result<string>.Success("Delete Success");
            }
            catch (Exception ex)
            {
                return Result<string>.SystemError($"Error deleting inventory: {ex.Message}");
            }
        }

        public async Task<Result<List<BloodInventoryResModel>>> GetAll(CancellationToken ct)
        {
            try
            {
                var items = await _db.BloodInventories
                    .Where(bi => bi.DeletedAt == null)
                    .OrderByDescending(bi => bi.CreatedAt)
                    .ToListAsync(ct);
                return Result<List<BloodInventoryResModel>>.Success(
                    items.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return Result<List<BloodInventoryResModel>>.SystemError(
                    $"Error retrieving inventory: {ex.Message}");
            }
        }

        public async Task<Result<List<AvailableStockResModel>>> GetAvailableStock(int? hospitalId, string? bloodGroup, CancellationToken ct)
        {
            try
            {
                var query = _db.BloodInventories.Where(bi => bi.DeletedAt == null && bi.Status == "available");
                if (hospitalId.HasValue)
                {
                    query = query.Where(bi => bi.HospitalId == hospitalId.Value);
                }
                if (!string.IsNullOrWhiteSpace(bloodGroup))
                {
                    query = query.Where(bi => bi.BloodGroup.ToUpper() == bloodGroup.Trim().ToUpper());
                }
                var result = await query
                                    .GroupBy(bi => new {bi.HospitalId, bi.BloodGroup})
                                    .Select(g => new AvailableStockResModel
                                     {
                                         HospitalId = g.Key.HospitalId,
                                         BloodGroup = g.Key.BloodGroup,
                                         TotalUnits = g.Sum(x => x.Units),
                                         AvailableCount = g.Count()
                                     })
                                    .ToListAsync(ct);
                return Result<List<AvailableStockResModel>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<AvailableStockResModel>>.SystemError(
              $"Error getting available stock: {ex.Message}");
            }
        }

        public async Task<Result<List<BloodInventoryResModel>>> GetByHospital(int hospitalId, CancellationToken ct)
        {
            try
            {
                var items = await _db.BloodInventories
                                       .Where(bi => bi.HospitalId == hospitalId && bi.DeletedAt == null)
                                       .OrderByDescending(bi => bi.CreatedAt)
                                       .ToListAsync(ct);
                return Result<List<BloodInventoryResModel>>.Success(items.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return Result<List<BloodInventoryResModel>>.SystemError(
                $"Error retrieving inventory: {ex.Message}");
            }
        }

        public async Task<Result<BloodInventoryResModel>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var item = await _db.BloodInventories
                                        .FirstOrDefaultAsync(bi => bi.Id == id && bi.DeletedAt == null, ct);
                if(item == null)
                    return Result<BloodInventoryResModel>.NotFound("Inventory not found.");

                return Result<BloodInventoryResModel>.Success(ToResponse(item));
            }
            catch(Exception ex) 
            {
                return Result<BloodInventoryResModel>.SystemError(
                $"Error retrieving inventory: {ex.Message}");
            }
        }

        public async Task<Result<int>> RunStockTake(int? hospitalId, CancellationToken cancellationToken)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var query = _db.BloodInventories.Where(bi => bi.DeletedAt == null 
                && bi.Status == "available" && bi.ExpiredAt.HasValue && bi.ExpiredAt.Value < today);

                if (hospitalId.HasValue)
                {
                    query = query.Where(bi => bi.HospitalId == hospitalId.Value);

                }
                var expiredItems = await query.ToListAsync();

                foreach(var item in expiredItems)
                {
                    item.Status = EnumBloodInventoryStatus.Expired.ToDatabaseValue();
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(expiredItems.Count,
                    $"{expiredItems.Count} unit(s) marked as expired.");
            }
            catch(Exception ex ) 
            {
                return Result<int>.SystemError($"Error during stock take: {ex.Message}");
            }
        }

        public async Task<Result<BloodInventoryResModel>> UseFromInventory(
    int inventoryId, int requestId, CancellationToken ct)
        {
            try
            {
                var inventory = await _db.BloodInventories
                    .FirstOrDefaultAsync(bi => bi.Id == inventoryId && bi.DeletedAt == null, ct);
                if (inventory is null)
                    return Result<BloodInventoryResModel>.NotFound("Inventory record not found.");
                if (!string.Equals(inventory.Status, "available", StringComparison.OrdinalIgnoreCase))
                    return Result<BloodInventoryResModel>.ValidationError(
                        "Only available blood units can be used.");
                var request = await _db.BloodRequests
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedAt == null, ct);
                if (request is null)
                    return Result<BloodInventoryResModel>.NotFound("Blood request not found.");
                if (!string.Equals(inventory.BloodGroup, request.BloodGroup, StringComparison.OrdinalIgnoreCase))
                    return Result<BloodInventoryResModel>.ValidationError(
                        "Blood group mismatch between inventory and request.");
                inventory.Status = EnumBloodInventoryStatus.Used.ToDatabaseValue();
                inventory.RequestId = requestId;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
                return Result<BloodInventoryResModel>.Success(ToResponse(inventory),
                    "Blood unit marked as used.");
            }
            catch (Exception ex)
            {
                return Result<BloodInventoryResModel>.SystemError(
                    $"Error using from inventory: {ex.Message}");
            }
        }

        private static BloodInventoryResModel ToResponse(
        Database.AppDbContextModels.BloodInventory item)
        {
            return new BloodInventoryResModel
            {
                Id = item.Id,
                DonationId = item.DonationId,
                HospitalId = item.HospitalId,
                BloodGroup = item.BloodGroup,
                Units = item.Units,
                CollectedAt = item.CollectedAt,
                ExpiredAt = item.ExpiredAt,
                Status = item.Status,
                RequestId = item.RequestId,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }
    }
}
