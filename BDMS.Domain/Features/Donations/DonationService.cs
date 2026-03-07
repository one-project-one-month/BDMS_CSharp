using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donation;

public class DonationService
{
    private readonly AppDbContext _db;
    public DonationService(AppDbContext db)
        {
            _db = db;
        }

    public async Task<Result<List<DonationRespModel>>> GetAllDonationsAsync()
    {
        try
        {
            var donations = await _db.Donations
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
} 
