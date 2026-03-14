using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.Donation;

public class DonationService : IDonationService
{
    private readonly AppDbContext _db;
    public DonationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DonationRespModel>>> GetAllDonations()
    {
        try
        {
            var donations = await _db.Donations
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
            if (donations.Count == 0)
            {
                return Result<List<DonationRespModel>>.NotFound("Cannot find the donation");
            }

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
                DeletedAt = a.DeletedAt,
                ApprovedByNavigation = a.ApprovedByNavigation,
                BloodInventory = a.BloodInventory,
                BloodRequest = a.BloodRequest,
                CreatedByNavigation = a.CreatedByNavigation,
                Donor = a.Donor,
                Hospital = a.Hospital,
                MedicalRecord = a.MedicalRecord
            }).ToList();

            return Result<List<DonationRespModel>>.Success(result, "Success");
        }
        catch (Exception ex)
        {

            return Result<List<DonationRespModel>>.SystemError($"Error retriving Donation : {ex.Message}");
        }
    }

    public async Task<Result<DonationRespModel>> CreateDonation(DonationCreateReqModel reqModel)
    {
        try
        {
            var donation = new BDMS.Database.AppDbContextModels.Donation()
            {
                DonorId = reqModel.DonorId,
                HospitalId = reqModel.HospitalId,
                BloodRequestId = reqModel.BloodRequestId,
                CreatedBy = reqModel.CreatedBy,
                DonationCode = reqModel.DonationCode,
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
                CreatedAt = donation.CreatedAt,
                ApprovedByNavigation = donation.ApprovedByNavigation,
                BloodInventory = donation.BloodInventory,
                BloodRequest = donation.BloodRequest,
                CreatedByNavigation = donation.CreatedByNavigation,
                Donor = donation.Donor,
                Hospital = donation.Hospital,
                MedicalRecord = donation.MedicalRecord
            };
            return Result<DonationRespModel>.Success(resp, "Donation is created successfully!");

        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error in creating donation {ex.Message}");
        }
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
                DeletedAt = donation.DeletedAt,
                ApprovedByNavigation = donation.ApprovedByNavigation,
                BloodInventory = donation.BloodInventory,
                BloodRequest = donation.BloodRequest,
                CreatedByNavigation = donation.CreatedByNavigation,
                Donor = donation.Donor,
                Hospital = donation.Hospital,
                MedicalRecord = donation.MedicalRecord,
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
                DeletedAt = donation.DeletedAt,
                ApprovedByNavigation = donation.ApprovedByNavigation,
                BloodInventory = donation.BloodInventory,
                BloodRequest = donation.BloodRequest,
                CreatedByNavigation = donation.CreatedByNavigation,
                Donor = donation.Donor,
                Hospital = donation.Hospital,
                MedicalRecord = donation.MedicalRecord,
            };

            return Result<DonationRespModel>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<DonationRespModel>.SystemError($"Error deleting donation : {ex.Message}");
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
}
