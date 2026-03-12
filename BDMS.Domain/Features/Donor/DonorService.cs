using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.Donor;

public class DonorService : IDonorService
{
    private readonly AppDbContext _db;

    public DonorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DonorRespModel>>> GetAllDonors()
    {
        try
        {
            var donors = await _db.Donors
            .Where(x => x.IsActive == true)
        .ToListAsync();

            var result = donors.Select(d => new DonorRespModel
            {
                Id = d.Id,
                UserId = d.UserId,
                NicNo = d.NicNo,
                DateOfBirth = d.DateOfBirth,
                Gender = d.Gender,
                BloodGroup = d.BloodGroup,
                LastDonationDate = d.LastDonationDate,
                Remarks = d.Remarks,
                EmergencyContact = d.EmergencyContact,
                EmergencyPhone = d.EmergencyPhone,
                Address = d.Address,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                DeletedAt = d.DeletedAt
            }).ToList();

            return Result<List<DonorRespModel>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<DonorRespModel>>.SystemError($"Error retrieving Donors: {ex.Message}");
        }

    }

    public async Task<Result<DonorRespModel>> CreateDonor(DonorReqModel reqModel)
    {
        try
        {
            var donor = new BDMS.Database.AppDbContextModels.Donor()
            {
                UserId = reqModel.UserId,
                NicNo = reqModel.NicNo,
                DateOfBirth = reqModel.DateOfBirth,
                Gender = reqModel.Gender,
                BloodGroup = reqModel.BloodGroup,
                LastDonationDate = reqModel.LastDonationDate,
                Remarks = reqModel.Remarks,
                EmergencyContact = reqModel.EmergencyContact,
                EmergencyPhone = reqModel.EmergencyPhone,
                Address = reqModel.Address,
                IsActive = reqModel.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.Donors.AddAsync(donor);
            await _db.SaveChangesAsync();

            var resp = new DonorRespModel
            {
                Id = donor.Id,
                UserId = donor.UserId,
                NicNo = donor.NicNo,
                DateOfBirth = donor.DateOfBirth,
                Gender = donor.Gender,
                BloodGroup = donor.BloodGroup,
                LastDonationDate = donor.LastDonationDate,
                Remarks = donor.Remarks,
                EmergencyContact = donor.EmergencyContact,
                EmergencyPhone = donor.EmergencyPhone,
                Address = donor.Address,
                IsActive = donor.IsActive,
                CreatedAt = donor.CreatedAt,
                UpdatedAt = donor.UpdatedAt
            };

            return Result<DonorRespModel>.Success(resp, "Donor created successfully");
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error creating Donor: {ex.Message}");
        }
    }
    public async Task<Result<DonorRespModel>> GetDonorById(int donorId)
    {
        try
        {
            var donor = await _db.Donors
            .FirstOrDefaultAsync(d => d.Id == donorId
            && d.IsActive == true);

            if (donor == null)
            {
                return Result<DonorRespModel>.NotFound("Donor not found");
            }

            var result = new DonorRespModel
            {
                Id = donor.Id,
                UserId = donor.UserId,
                NicNo = donor.NicNo,
                DateOfBirth = donor.DateOfBirth,
                Gender = donor.Gender,
                BloodGroup = donor.BloodGroup,
                LastDonationDate = donor.LastDonationDate,
                Remarks = donor.Remarks,
                EmergencyContact = donor.EmergencyContact,
                EmergencyPhone = donor.EmergencyPhone,
                Address = donor.Address,
                IsActive = donor.IsActive,
                CreatedAt = donor.CreatedAt,
                UpdatedAt = donor.UpdatedAt,
                DeletedAt = donor.DeletedAt
            };

            return Result<DonorRespModel>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error creating Donor: {ex.Message}");
        }
    }

    public async Task<Result<DonorRespModel>> UpdateDonor(DonorReqModel reqModel)
    {
        try
        {
            var donor = _db.Donors
            .FirstOrDefault(d => d.Id == reqModel.Id
            && d.IsActive == true);

            if (donor == null)
            {
                return Result<DonorRespModel>.NotFound("Donor not found");
            }

            donor.UserId = reqModel.UserId;
            donor.NicNo = reqModel.NicNo;
            donor.DateOfBirth = reqModel.DateOfBirth;
            donor.Gender = reqModel.Gender;
            donor.BloodGroup = reqModel.BloodGroup;
            donor.LastDonationDate = reqModel.LastDonationDate;
            donor.Remarks = reqModel.Remarks;
            donor.EmergencyContact = reqModel.EmergencyContact;
            donor.EmergencyPhone = reqModel.EmergencyPhone;
            donor.Address = reqModel.Address;
            donor.IsActive = reqModel.IsActive;
            donor.UpdatedAt = DateTime.UtcNow;

            _db.Entry(donor).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var result = new DonorRespModel()
            {
                UserId = donor.UserId,
                NicNo = donor.NicNo,
                DateOfBirth = donor.DateOfBirth,
                Gender = donor.Gender,
                BloodGroup = donor.BloodGroup,
                LastDonationDate = donor.LastDonationDate,
                Remarks = donor.Remarks,
                EmergencyContact = donor.EmergencyContact,
                EmergencyPhone = donor.EmergencyPhone,
                Address = donor.Address,
                IsActive = donor.IsActive,
                UpdatedAt = donor.UpdatedAt
            };

            return Result<DonorRespModel>.Success(result, "Donor updated successfully");
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error updating Donor: {ex.Message}");
        }
    }

    public async Task<Result<DonorRespModel>> DeleteDonor(int donorId)
    {
        try
        {
            var donor = await _db.Donors
            .FirstOrDefaultAsync(d => d.Id == donorId
            && d.IsActive == true);

            if (donor == null)
            {
                return Result<DonorRespModel>.NotFound("Donor not found");
            }

            donor.IsActive = false;
            donor.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Result<DonorRespModel>.Success(null, "Donor deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error deleting Donor: {ex.Message}");
        }
    }
}
