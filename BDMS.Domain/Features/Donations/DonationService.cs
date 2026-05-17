using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodInventory;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donations.Commands;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Domain.Features.Donations.Queries;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;

namespace BDMS.Domain.Features.Donation;

public class DonationService : IDonationService
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> HospitalCodeLocks = new();
    private sealed class LockReleaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public LockReleaser(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public ValueTask DisposeAsync()
        {
            _semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }

    private readonly AppDbContext _db;

    private readonly IBloodInventoryService _inventoryService;
    public DonationService(AppDbContext db, IBloodInventoryService bloodInventoryService)
    {
        _db = db;
        _inventoryService = bloodInventoryService;
    }

    public async Task<Result<List<DonationRespModel>>> GetAllDonations()
    {
        try
        {
            var donations = await _db.Donations
                                    .Include(x => x.Donor)
                                        .ThenInclude(d => d.User)
                                    .Where(x => x.DeletedAt == null
                                             && x.Donor.DeletedAt == null
                                             && x.Donor.User.DeletedAt == null)
                                    .ToListAsync();
        
            var result = donations.Select(a => new DonationRespModel
            {
                Id = a.Id,
                DonorId = a.DonorId,
                HospitalId = a.HospitalId,
                BloodRequestId = a.BloodRequestId,
                CreatedBy = a.CreatedBy,
                DonationCode = a.DonationCode,
                BloodGroup = a.BloodGroup,
                UnitsDonated = a.UnitsDonated,
                DonationDate = a.DonationDate,
                Status = a.Status,
                ApprovedBy = a.ApprovedBy,
                ApprovedAt = a.ApprovedAt,
                Remarks = a.Remarks,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                DeletedAt = a.DeletedAt
            }).ToList();

            return Result<List<DonationRespModel>>.Success(result, "Success");
        }
        catch (Exception ex)
        {

            return Result<List<DonationRespModel>>.SystemError($"Error retrieving Donation : {ex.Message}");
        }
    }

    public async Task<Result<DonationRespModel>> CreateDonation(DonationCreateReqModel reqModel)
    {
        try
        {
            await using var lockReleaser = await AcquireHospitalLockAsync(reqModel.HospitalId, CancellationToken.None);

            var hospitalName = await _db.Hospitals
                .Where(h => h.Id == reqModel.HospitalId && h.DeletedAt == null)
                .Select(h => h.Name)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(hospitalName))
            {
                return Result<DonationRespModel>.ValidationError("Invalid hospital.");
            }

            var donationCode = await GenerateDonationCode(reqModel.HospitalId, hospitalName, reqModel.BloodGroup);

            var donation = new BDMS.Database.AppDbContextModels.Donation()
            {
                DonorId = reqModel.DonorId,
                HospitalId = reqModel.HospitalId,
                BloodRequestId = reqModel.BloodRequestId,
                CreatedBy = reqModel.CreatedBy,
                DonationCode = donationCode,
                BloodGroup = reqModel.BloodGroup,
                UnitsDonated = reqModel.UnitsDonated,
                DonationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Pending",
                Remarks = reqModel.Remarks,
                CreatedAt = DateTime.UtcNow,
            };

            await _db.Donations.AddAsync(donation);
            await _db.SaveChangesAsync();

            var resp = new DonationRespModel
            {
                Id = donation.Id,
                DonorId = donation.DonorId,
                HospitalId = donation.HospitalId,
                BloodRequestId = donation.BloodRequestId,
                DonationCode = donation.DonationCode,
                BloodGroup = donation.BloodGroup,
                UnitsDonated = donation.UnitsDonated,
                DonationDate = donation.DonationDate,
                Status = donation.Status,
                Remarks = donation.Remarks,
                CreatedAt = donation.CreatedAt
            };
            return Result<DonationRespModel>.Success(resp, "Donation is created successfully!");

        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error in creating donation {ex.Message}");
        }
    }

    private static async Task<IAsyncDisposable> AcquireHospitalLockAsync(int hospitalId, CancellationToken ct)
    {
        var hospitalLock = HospitalCodeLocks.GetOrAdd(hospitalId, _ => new SemaphoreSlim(1, 1));
        await hospitalLock.WaitAsync(ct);
        return new LockReleaser(hospitalLock);
    }

    private async Task<string> GenerateDonationCode(int hospitalId, string hospitalName, string bloodGroup)
    {
        var hospitalCode = Functions.NormalizeCodeSegment(hospitalName);
        var bloodTypeCode = Functions.NormalizeCodeSegment(bloodGroup);
        var prefix = $"{hospitalCode}_{bloodTypeCode}_";

        var existingCodes = await _db.Donations
            .Where(x => x.DeletedAt == null
                && x.HospitalId == hospitalId
                && x.BloodGroup == bloodGroup
                && x.DonationCode != null
                && x.DonationCode.StartsWith(prefix))
            .Select(x => x.DonationCode!)
            .ToListAsync();

        var maxSequence = 0;
        foreach (var code in existingCodes)
        {
            var suffix = code[prefix.Length..];
            if (int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out var seq) && seq > maxSequence)
            {
                maxSequence = seq;
            }
        }

        return $"{prefix}{(maxSequence + 1):D2}";
    }

    public async Task<Result<DonationRespModel>> UpdateDonation(DonationUpdateReqModel reqModel)
    {
        try
        {
            var donation = await _db.Donations
            .FirstOrDefaultAsync(x => x.Id == reqModel.Id && x.DeletedAt == null);

            if (donation is null)
            {
                return Result<DonationRespModel>.NotFound("Cannot find the donation to be updated.");
            }
            var previousStatus = donation.Status;

            donation.Id = reqModel.Id;
            donation.DonorId = reqModel.DonorId;
            donation.HospitalId = reqModel.HospitalId;
            donation.BloodRequestId = reqModel.BloodRequestId;
            donation.DonationCode = reqModel.DonationCode;
            donation.BloodGroup = reqModel.BloodGroup;
            donation.UnitsDonated = reqModel.UnitsDonated;
            donation.DonationDate = reqModel.DonationDate;
            donation.Status = reqModel.Status;
            donation.ApprovedBy = reqModel.ApprovedBy;
            donation.ApprovedAt = reqModel.ApprovedAt;
            donation.Remarks = reqModel.Remarks;
            donation.UpdatedAt = DateTime.UtcNow;
            _db.Entry(donation).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            //Add validation logic in here
            if (!string.Equals(previousStatus, "completed", StringComparison.OrdinalIgnoreCase)
               && string.Equals(reqModel.Status, "completed", StringComparison.OrdinalIgnoreCase))
            {
                await _inventoryService.AddtoInventory(donation.Id, CancellationToken.None);
            }

            var result = new DonationRespModel()
            {
                DonorId = donation.DonorId,
                HospitalId = donation.HospitalId,
                BloodRequestId = donation.BloodRequestId,
                CreatedBy = donation.CreatedBy,
                DonationCode = donation.DonationCode,
                BloodGroup = donation.BloodGroup,
                UnitsDonated = donation.UnitsDonated,
                DonationDate = donation.DonationDate,
                Status = donation.Status,
                ApprovedBy = donation.ApprovedBy,
                ApprovedAt = donation.ApprovedAt,
                Remarks = donation.Remarks,
                UpdatedAt = donation.UpdatedAt,
                DeletedAt = donation.DeletedAt
            };
            return Result<DonationRespModel>.Success(result, "Donation updated successfully!");

        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error in updating donation : {ex.Message}");
        }

    }

    public async Task<Result<DonationRespModel>> GetDonationById(int donationId)
    {
        try
        {
            var donation = await _db.Donations
            .FirstOrDefaultAsync(x => x.Id == donationId && x.DeletedAt == null);
            if (donation is null)
            {
                return Result<DonationRespModel>.NotFound("Donation not found.");
            }

            var result = new DonationRespModel()
            {
                Id = donationId,
                DonorId = donation.DonorId,
                HospitalId = donation.HospitalId,
                BloodRequestId = donation.BloodRequestId,
                CreatedBy = donation.CreatedBy,
                DonationCode = donation.DonationCode,
                BloodGroup = donation.BloodGroup,
                UnitsDonated = donation.UnitsDonated,
                DonationDate = donation.DonationDate,
                Status = donation.Status,
                ApprovedBy = donation.ApprovedBy,
                ApprovedAt = donation.ApprovedAt,
                Remarks = donation.Remarks,
                UpdatedAt = donation.UpdatedAt,
                DeletedAt = donation.DeletedAt
            };

            return Result<DonationRespModel>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error getting donation by Id : {ex.Message}");
        }
    }

    public async Task<Result<DonationRespModel>> DeleteDonation(int donationId)
    {
        try
        {
            var donation = await _db.Donations
            .FirstOrDefaultAsync(x => x.Id == donationId && x.DeletedAt == null);
            if (donation is null)
            {
                return Result<DonationRespModel>.NotFound("Donation not found.");
            }
            donation.DeletedAt = DateTime.UtcNow;
            _db.Entry(donation).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Result<DonationRespModel>.Success(new(), "Donation deleted successfully!");
        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error deleting donation : {ex.Message}");
        }
    }

    public async Task<Result<DonationRespModel>> UpdateDonationStatus(UpdateDonationStatusCommand reqModel)
    {
        if (reqModel.Status == EnumDonationStatus.None)
            return Result<DonationRespModel>.ValidationError("Status is required.");
        try
        {
            var donation = await _db.Donations
            .FirstOrDefaultAsync(x => x.Id == reqModel.Id && x.DeletedAt == null);

            if (donation is null)
            {
                return Result<DonationRespModel>.NotFound("Cannot find the donation to be updated.");
            }

            var previousStatus = donation.Status;

            donation.Status = reqModel.Status.ToDatabaseValue();
            donation.UpdatedAt = DateTime.UtcNow;
            _db.Entry(donation).State = EntityState.Modified;
            await _db.SaveChangesAsync();


            if (!string.Equals(previousStatus, "completed", StringComparison.OrdinalIgnoreCase)
               && string.Equals(reqModel.Status.ToDatabaseValue(), "completed", StringComparison.OrdinalIgnoreCase))
            {
                await _inventoryService.AddtoInventory(donation.Id, CancellationToken.None);
            }

            var result = new DonationRespModel()
            {
                DonorId = donation.DonorId,
                HospitalId = donation.HospitalId,
                BloodRequestId = donation.BloodRequestId,
                CreatedBy = donation.CreatedBy,
                DonationCode = donation.DonationCode,
                BloodGroup = donation.BloodGroup,
                UnitsDonated = donation.UnitsDonated,
                DonationDate = donation.DonationDate,
                Status = donation.Status,
                ApprovedBy = donation.ApprovedBy,
                ApprovedAt = donation.ApprovedAt,
                Remarks = donation.Remarks,
                UpdatedAt = donation.UpdatedAt,
                DeletedAt = donation.DeletedAt
            };
            return Result<DonationRespModel>.Success(result, "Donation Status updated successfully!");

        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error in updating donation status: {ex.Message}");
        }
    }

    public async Task<Result<List<DonationRespModel>>> GetDonationByDateAndHospi(GetDonationByDateAndHospitalQuery reqModel)
    {
        try
        {
            var query = _db.Donations.Where(x => x.DeletedAt == null);
            if (reqModel.HospitalId > 0)
            {
                query = query.Where(x => x.HospitalId == reqModel.HospitalId);
            }
            if(reqModel.DonationDate is not null)
            {
               query = query.Where(x => x.DonationDate == reqModel.DonationDate);
            }
            if (query is null)
            {
                return Result<List<DonationRespModel>>.NotFound("Donation not found.");
            }
            var donation = await query.ToListAsync();

            var result = donation.Select(d => new DonationRespModel
            {
                Id = d.Id,
                DonorId = d.DonorId,
                HospitalId = d.HospitalId,
                BloodRequestId = d.BloodRequestId,
                CreatedBy = d.CreatedBy,
                DonationCode = d.DonationCode,
                BloodGroup = d.BloodGroup,
                UnitsDonated = d.UnitsDonated,
                DonationDate = d.DonationDate,
                Status = d.Status,
                ApprovedBy = d.ApprovedBy,
                ApprovedAt = d.ApprovedAt,
                Remarks = d.Remarks,
                UpdatedAt = d.UpdatedAt,
                DeletedAt = d.DeletedAt
            }).ToList();

            return Result<List<DonationRespModel>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<DonationRespModel>>.SystemError($"Error deleting donation : {ex.Message}");
        }
    }

}
