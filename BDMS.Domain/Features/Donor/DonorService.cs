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
            .Where(x => x.DeletedAt == null)
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
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            //using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == reqModel.UserId && u.IsActive == true);
                if (user == null)
                {
                    return Result<DonorRespModel>.NotFound("User not found.");
                }
                var donorRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "donor");
                if (donorRole == null)
                {
                    return Result<DonorRespModel>.NotFound("Donor role not found.");
                }


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

                if (user.RoleId != donorRole.Id)
                {
                    user.RoleId = donorRole.Id;
                    user.UpdatedAt = DateTime.Now;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                }

                //await transaction.CommitAsync();

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
        );
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
            return Result<DonorRespModel>.SystemError($"Error retrieving Donor: {ex.Message}");
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


    public async Task<Result<DonorRespModel>> UpdateDonorStatus(int donorId, bool isActive)
    {
        try
        {
            var donor = await _db.Donors.FirstOrDefaultAsync(d => d.Id == donorId);
            if (donor == null)
            {
                return Result<DonorRespModel>.NotFound("Donor not found");
            }

            donor.IsActive = isActive;
            donor.UpdatedAt = DateTime.UtcNow;
            donor.DeletedAt = isActive ? null : DateTime.UtcNow;

            await _db.SaveChangesAsync();

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

            return Result<DonorRespModel>.Success(result, isActive ? "Donor activated successfully" : "Donor deactivated successfully");
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error updating donor status: {ex.Message}");
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

            return Result<DonorRespModel>.Success(new DonorRespModel { Id = donorId }, "Donor deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<DonorRespModel>.SystemError($"Error deleting Donor: {ex.Message}");
        }
    }
}
