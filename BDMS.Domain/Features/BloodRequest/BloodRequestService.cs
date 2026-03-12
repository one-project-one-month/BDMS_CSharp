using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.BloodRequest;

public class BloodRequestService : IBloodRequestService
{
    private readonly AppDbContext _db;
    private static readonly string[] AllowedUrgencies = ["low", "medium", "high", "critical"];

    public BloodRequestService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<BloodRequestRespModel>>> GetAll(CancellationToken ct)
    {
        try
        {
            var requests = await _db.BloodRequests
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return Result<List<BloodRequestRespModel>>.Success(requests.Select(ToResponse).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<BloodRequestRespModel>>.SystemError($"Error retrieving blood requests: {ex.Message}");
        }
    }

    public async Task<Result<BloodRequestRespModel>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var request = await _db.BloodRequests.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);
            if (request == null)
                return Result<BloodRequestRespModel>.NotFound("Blood request not found.");

            return Result<BloodRequestRespModel>.Success(ToResponse(request));
        }
        catch (Exception ex)
        {
            return Result<BloodRequestRespModel>.SystemError($"Error retrieving blood request: {ex.Message}");
        }
    }

    public async Task<Result<BloodRequestRespModel>> Create(CreateBloodRequestCommand command, CancellationToken ct)
    {
        var bloodGroup = command.BloodGroup.ToBloodGroupEnum();
        if (bloodGroup == EnumBloodGroup.None)
            return Result<BloodRequestRespModel>.ValidationError("Invalid blood group.");

        if (!IsUrgencyValid(command.Urgency))
            return Result<BloodRequestRespModel>.ValidationError("Invalid urgency. Allowed values: low, medium, high, critical.");

        var entity = new Database.AppDbContextModels.BloodRequest
        {
            UserId = command.UserId,
            HospitalId = command.HospitalId,
            PatientName = command.PatientName,
            BloodGroup = bloodGroup.ToDatabaseValue(),
            UnitsRequired = command.UnitsRequired <= 0 ? 1 : command.UnitsRequired,
            ContactPhone = command.ContactPhone,
            Urgency = command.Urgency.ToLowerInvariant(),
            RequiredDate = command.RequiredDate,
            Status = EnumBloodRequestStatus.Pending.ToDatabaseValue(),
            Reason = command.Reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            await _db.BloodRequests.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return Result<BloodRequestRespModel>.Success(ToResponse(entity), "Blood request created successfully.");
        }
        catch (Exception ex)
        {
            return Result<BloodRequestRespModel>.SystemError($"Error creating blood request: {ex.Message}");
        }
    }

    public async Task<Result<BloodRequestRespModel>> Update(UpdateBloodRequestCommand command, CancellationToken ct)
    {
        var bloodGroup = command.BloodGroup.ToBloodGroupEnum();
        if (bloodGroup == EnumBloodGroup.None)
            return Result<BloodRequestRespModel>.ValidationError("Invalid blood group.");

        if (!IsUrgencyValid(command.Urgency))
            return Result<BloodRequestRespModel>.ValidationError("Invalid urgency. Allowed values: low, medium, high, critical.");

        try
        {
            var entity = await _db.BloodRequests.FirstOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null, ct);
            if (entity == null)
                return Result<BloodRequestRespModel>.NotFound("Blood request not found.");

            entity.UserId = command.UserId;
            entity.HospitalId = command.HospitalId;
            entity.PatientName = command.PatientName;
            entity.BloodGroup = bloodGroup.ToDatabaseValue();
            entity.UnitsRequired = command.UnitsRequired <= 0 ? 1 : command.UnitsRequired;
            entity.ContactPhone = command.ContactPhone;
            entity.Urgency = command.Urgency.ToLowerInvariant();
            entity.RequiredDate = command.RequiredDate;
            entity.Reason = command.Reason;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Result<BloodRequestRespModel>.Success(ToResponse(entity), "Blood request updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<BloodRequestRespModel>.SystemError($"Error updating blood request: {ex.Message}");
        }
    }

    public async Task<Result<string>> Delete(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _db.BloodRequests.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);
            if (entity == null)
                return Result<string>.NotFound("Blood request not found.");

            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Result<string>.Success(null, "Deleting Successful.");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"Error deleting blood request: {ex.Message}");
        }
    }

    public async Task<Result<BloodRequestRespModel>> UpdateStatus(UpdateBloodRequestStatusCommand command, CancellationToken ct)
    {
        if (command.Status == EnumBloodRequestStatus.None)
            return Result<BloodRequestRespModel>.ValidationError("Status is required.");

        try
        {
            var entity = await _db.BloodRequests.FirstOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null, ct);
            if (entity == null)
                return Result<BloodRequestRespModel>.NotFound("Blood request not found.");

            var currentStatus = entity.Status.ToEnumOrDefault(EnumBloodRequestStatus.None);
            if (currentStatus == EnumBloodRequestStatus.Fulfilled || currentStatus == EnumBloodRequestStatus.Cancelled)
                return Result<BloodRequestRespModel>.ValidationError("Status cannot be changed from fulfilled or cancelled.");

            if (command.Status is EnumBloodRequestStatus.Approved or EnumBloodRequestStatus.Fulfilled)
            {
                if (!command.DonorId.HasValue)
                    return Result<BloodRequestRespModel>.ValidationError("DonorId is required when approving or fulfilling a blood request.");

                var donor = await _db.Donors.FirstOrDefaultAsync(x => x.Id == command.DonorId && x.IsActive && x.DeletedAt == null, ct);
                if (donor == null)
                    return Result<BloodRequestRespModel>.NotFound("Referenced donor not found.");

                if (!string.Equals(donor.BloodGroup, entity.BloodGroup, StringComparison.OrdinalIgnoreCase))
                    return Result<BloodRequestRespModel>.ValidationError("Donor blood group does not match request blood group.");

                entity.ApprovedBy = donor.UserId;
                entity.ApprovedAt = DateTime.UtcNow;
            }
            else if (command.Status == EnumBloodRequestStatus.Rejected)
            {
                entity.ApprovedBy = null;
                entity.ApprovedAt = null;
            }

            entity.Status = command.Status.ToDatabaseValue();
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Result<BloodRequestRespModel>.Success(ToResponse(entity), "Blood request status updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<BloodRequestRespModel>.SystemError($"Error updating blood request status: {ex.Message}");
        }
    }

    private static bool IsUrgencyValid(string urgency)
        => AllowedUrgencies.Contains((urgency ?? string.Empty).Trim().ToLowerInvariant());

    private static BloodRequestRespModel ToResponse(Database.AppDbContextModels.BloodRequest request)
    {
        return new BloodRequestRespModel
        {
            Id = request.Id,
            UserId = request.UserId,
            HospitalId = request.HospitalId,
            BloodRequestCode = request.BloodRequestCode,
            PatientName = request.PatientName,
            BloodGroup = request.BloodGroup.ToBloodGroupEnum(),
            UnitsRequired = request.UnitsRequired,
            ContactPhone = request.ContactPhone,
            Urgency = request.Urgency,
            RequiredDate = request.RequiredDate,
            Status = request.Status.ToEnumOrDefault(EnumBloodRequestStatus.None),
            Reason = request.Reason,
            ApprovedBy = request.ApprovedBy,
            ApprovedAt = request.ApprovedAt,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            DeletedAt = request.DeletedAt
        };
    }
}
