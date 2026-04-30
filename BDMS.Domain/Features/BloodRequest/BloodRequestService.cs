using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;

namespace BDMS.Domain.Features.BloodRequest;

public class BloodRequestService : IBloodRequestService
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

        try
        {
            await using var lockReleaser = await AcquireHospitalLockAsync(command.HospitalId, ct);

            var now = DateTime.UtcNow;
            var hospitalName = await _db.Hospitals
                .Where(h => h.Id == command.HospitalId && h.DeletedAt == null)
                .Select(h => h.Name)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(hospitalName))
                return Result<BloodRequestRespModel>.ValidationError("Invalid hospital.");

            var generatedCode = await GenerateBloodRequestCode(command.HospitalId, hospitalName, bloodGroup.ToDatabaseValue(), ct);

            var entity = new Database.AppDbContextModels.BloodRequest
            {
                UserId = command.UserId,
                HospitalId = command.HospitalId,
                BloodRequestCode = generatedCode,
                PatientName = command.PatientName,
                BloodGroup = bloodGroup.ToDatabaseValue(),
                UnitsRequired = command.UnitsRequired <= 0 ? 1 : command.UnitsRequired,
                ContactPhone = command.ContactPhone,
                Urgency = command.Urgency.ToDatabaseValue(),
                RequiredDate = command.RequiredDate,
                Status = EnumBloodRequestStatus.Pending.ToDatabaseValue(),
                Reason = command.Reason,
                CreatedAt = now,
                UpdatedAt = now
            };

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

            var immutableCode = entity.BloodRequestCode;

            //var currentStatus = entity.Status.ToEnumOrDefault(EnumBloodRequestStatus.None);
            //if (currentStatus != EnumBloodRequestStatus.Pending)
            //    return Result<BloodRequestRespModel>.ValidationError("Only pending blood requests can be updated. Use update status endpoint to change request status.");

            entity.UserId = command.UserId;
            entity.HospitalId = command.HospitalId;
            entity.PatientName = command.PatientName;
            entity.BloodGroup = bloodGroup.ToDatabaseValue();
            entity.UnitsRequired = command.UnitsRequired <= 0 ? 1 : command.UnitsRequired;
            entity.ContactPhone = command.ContactPhone;
            entity.Urgency = command.Urgency.ToDatabaseValue();
            entity.RequiredDate = command.RequiredDate;
            entity.Reason = command.Reason;
            entity.BloodRequestCode = immutableCode;
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

            return Result<string>.Success("Deleting Successful.");
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
                int? approvedByUserId = command.ApprovedByUserId > 0 ? command.ApprovedByUserId : null;

                if (!command.DonorId.HasValue && command.Status == EnumBloodRequestStatus.Fulfilled)
                    return Result<BloodRequestRespModel>.ValidationError("DonorId is required when fulfilling a blood request.");

                if (command.DonorId.HasValue)
                {
                    var donor = await _db.Donors.FirstOrDefaultAsync(x => x.Id == command.DonorId && x.IsActive && x.DeletedAt == null, ct);
                    if (donor == null)
                        return Result<BloodRequestRespModel>.NotFound("Referenced donor not found.");

                    if (!string.Equals(donor.BloodGroup, entity.BloodGroup, StringComparison.OrdinalIgnoreCase))
                        return Result<BloodRequestRespModel>.ValidationError("Donor blood group does not match request blood group.");

                    if (!approvedByUserId.HasValue)
                        approvedByUserId = donor.UserId;
                }

                entity.ApprovedBy = approvedByUserId;
                entity.ApprovedAt = approvedByUserId.HasValue ? DateTime.UtcNow : null;
            }

            if (command.Status == EnumBloodRequestStatus.Approved && !entity.RequiredDate.HasValue)
                return Result<BloodRequestRespModel>.ValidationError("RequiredDate is required when approving a blood request.");

            if (command.Status == EnumBloodRequestStatus.Fulfilled)
            {
                var availableUnits = await _db.BloodInventories
                    .Where(bi => bi.DeletedAt == null
                        && bi.Status == "available"
                        && bi.HospitalId == entity.HospitalId
                        && string.Equals(bi.BloodGroup, entity.BloodGroup))
                    .OrderBy(bi => bi.ExpiredAt)  // FIFO: use oldest first
                    .Take(entity.UnitsRequired)
                    .ToListAsync(ct);
                if (availableUnits.Count < entity.UnitsRequired)
                    return Result<BloodRequestRespModel>.ValidationError(
                        $"Insufficient stock. Available: {availableUnits.Count}, Required: {entity.UnitsRequired}");
                foreach (var unit in availableUnits)
                {
                    unit.Status = "used";
                    unit.RequestId = entity.Id;
                    unit.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (command.Status == EnumBloodRequestStatus.Rejected)
            {
                entity.ApprovedBy = null;
                entity.ApprovedAt = null;
            }

            entity.Status = command.Status.ToDatabaseValue();
            entity.UpdatedAt = DateTime.UtcNow;

            if (command.Status == EnumBloodRequestStatus.Approved)
            {
                await EnsureAppointmentStartedForApprovedRequest(entity, ct);
            }

            await _db.SaveChangesAsync(ct);

            return Result<BloodRequestRespModel>.Success(ToResponse(entity), "Blood request status updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<BloodRequestRespModel>.SystemError($"Error updating blood request status: {ex.Message}");
        }
    }

    private static bool IsUrgencyValid(EnumBloodRequestUrgency urgency)
        => urgency != EnumBloodRequestUrgency.None;

    private static async Task<IAsyncDisposable> AcquireHospitalLockAsync(int hospitalId, CancellationToken ct)
    {
        var hospitalLock = HospitalCodeLocks.GetOrAdd(hospitalId, _ => new SemaphoreSlim(1, 1));
        await hospitalLock.WaitAsync(ct);
        return new LockReleaser(hospitalLock);
    }

    private async Task<string> GenerateBloodRequestCode(int hospitalId, string hospitalName, string bloodGroup, CancellationToken ct)
    {
        var hospitalCode = Functions.NormalizeCodeSegment(hospitalName);
        var bloodTypeCode = Functions.NormalizeCodeSegment(bloodGroup);
        var prefix = $"{hospitalCode}_{bloodTypeCode}_";

        var existingCodes = await _db.BloodRequests
            .Where(x => x.DeletedAt == null
                && x.HospitalId == hospitalId
                && x.BloodGroup == bloodGroup
                && x.BloodRequestCode != null
                && x.BloodRequestCode.StartsWith(prefix))
            .Select(x => x.BloodRequestCode!)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var code in existingCodes)
        {
            var suffix = code[prefix.Length..];
            if (int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out var seq) && seq > maxSequence)
                maxSequence = seq;
        }

        return $"{prefix}{(maxSequence + 1):D2}";
    }

    private async Task EnsureAppointmentStartedForApprovedRequest(Database.AppDbContextModels.BloodRequest request, CancellationToken ct)
    {
        var cancelledStatus = EnumAppointmentStatus.Cancelled.ToString().ToLowerInvariant();
        var hasOpenAppointment = await _db.Appointments
            .AnyAsync(x =>
                x.BloodRequestId == request.Id &&
                x.DeletedAt == null &&
                x.Status.ToLower() != cancelledStatus, ct);

        if (hasOpenAppointment)
            return;

        var now = DateTime.UtcNow;
        var appointment = new Database.AppDbContextModels.Appointment
        {
            UserId = request.UserId,
            HospitalId = request.HospitalId,
            BloodRequestId = request.Id,
            AppointmentDate = request.RequiredDate!.Value,
            AppointmentTime = new TimeOnly(now.Hour, now.Minute),
            Status = EnumAppointmentStatus.Scheduled.ToString().ToLowerInvariant(),
            Remarks = "Auto-created when blood request was approved",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _db.Appointments.AddAsync(appointment, ct);
    }

    private static BloodRequestRespModel ToResponse(Database.AppDbContextModels.BloodRequest request)
    {
        return new BloodRequestRespModel
        {
            Id = request.Id,
            UserId = request.UserId,
            HospitalId = request.HospitalId,
            BloodRequestCode = request.BloodRequestCode,
            PatientName = request.PatientName,
            BloodGroup = request.BloodGroup.ToBloodGroupEnum().ToString().ToLowerInvariant(),
            UnitsRequired = request.UnitsRequired,
            ContactPhone = request.ContactPhone,
            Urgency = request.Urgency,
            RequiredDate = request.RequiredDate,
            Status = request.Status.ToEnumOrDefault(EnumBloodRequestStatus.None).ToString().ToLowerInvariant(),
            Reason = request.Reason,
            ApprovedBy = request.ApprovedBy,
            ApprovedAt = request.ApprovedAt,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            DeletedAt = request.DeletedAt
        };
    }
}
