using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Hospital.Commands;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Hospital;

public class HospitalService : IHospitalService
{
    private readonly AppDbContext _db;

    public HospitalService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<HospitalRespModel>>> GetAllHospitals()
    {
        try
        {
            var hspital = await _db.Hospitals
            .Where(x => x.IsActive == true)
        .ToListAsync();

            var result = hspital.Select(h => new HospitalRespModel
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                Phone = h.Phone,
                Email = h.Email,
                IsActive = h.IsActive,
                IsVerified = h.IsVerified,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
                DeletedAt = h.DeletedAt
            }).ToList();

            return Result<List<HospitalRespModel>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<HospitalRespModel>>.SystemError($"Error retrieving Hospitals: {ex.Message}");
        }
    }

    public async Task<Result<HospitalRespModel>> GetHospitalById(int hospitalId)
    {
        try
        {
            var hospital = await _db.Hospitals
            .FirstOrDefaultAsync(d => d.Id == hospitalId
            && d.IsActive == true);

            if (hospital == null)
            {
                return Result<HospitalRespModel>.NotFound("Hospital not found");
            }

            var result = new HospitalRespModel
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Address = hospital.Address,
                Phone = hospital.Phone,
                Email = hospital.Email,
                IsActive = hospital.IsActive,
                IsVerified = hospital.IsVerified,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt,
                DeletedAt = hospital.DeletedAt
            };

            return Result<HospitalRespModel>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<HospitalRespModel>.SystemError($"Error retrieving Hospital: {ex.Message}");
        }
    }

    public async Task<Result<HospitalRespModel>> DeleteHospital(int hospitalId)
    {
        try
        {
            var hospital = await _db.Hospitals
            .FirstOrDefaultAsync(d => d.Id == hospitalId
            && d.IsActive == true);

            if (hospital == null)
            {
                return Result<HospitalRespModel>.NotFound("Hospital not found");
            }

            hospital.IsActive = false;
            hospital.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Result<HospitalRespModel>.Success(new HospitalRespModel { Id = hospitalId }, "Hospital deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<HospitalRespModel>.SystemError($"Error deleting Hospital: {ex.Message}");
        }
    }

    public async Task<Result<HospitalRespModel>> UpdateHospital(UpdateHospitalCommand reqModel)
    {
        try
        {
            var hospital = await _db.Hospitals
            .FirstOrDefaultAsync(d => d.Id == reqModel.Id
            && d.IsActive == true);

            if (hospital == null)
            {
                return Result<HospitalRespModel>.NotFound("Hospital not found");
            }

            hospital.Name = reqModel.Name;
            hospital.Address = reqModel.Address;
            hospital.Phone = reqModel.Phone;
            hospital.Email = reqModel.Email;
            hospital.IsVerified = reqModel.IsVerified;
            hospital.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var result = new HospitalRespModel
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Address = hospital.Address,
                Phone = hospital.Phone,
                Email = hospital.Email,
                IsActive = hospital.IsActive,
                IsVerified = hospital.IsVerified,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt,
                DeletedAt = hospital.DeletedAt
            };
            return Result<HospitalRespModel>.Success(result, "Hospital updated successfully");
        }
        catch (Exception ex)
        {
            return Result<HospitalRespModel>.SystemError($"Error updating Hospital: {ex.Message}");
        }
    }
}
