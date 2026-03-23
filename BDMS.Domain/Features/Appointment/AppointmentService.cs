using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Domain.Features.Appointment.Queries;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using AppointmentEntity = BDMS.Database.AppDbContextModels.Appointment;

namespace BDMS.Domain.Features.Appointment;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;

    public AppointmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<AppointmentRespModel>>> GetAllAppointments(int? hospitalId, DateOnly? appointmentDate, CancellationToken ct)
    {
        try
        {
            var query = _db.Appointments.Where(x => x.DeletedAt == null);

            if (hospitalId.HasValue)
            {
                query = query.Where(x => x.HospitalId == hospitalId.Value);
            }

            if (appointmentDate.HasValue)
            {
                query = query.Where(x => x.AppointmentDate == appointmentDate.Value);
            }
            
            var result = await query
                .AsNoTracking()
                .Select(x => new AppointmentRespModel()
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    HospitalId = x.HospitalId,
                    DonationId = x.DonationId,
                    BloodRequestId = x.BloodRequestId,
                    AppointmentDate = x.AppointmentDate,
                    AppointmentTime = x.AppointmentTime,
                    Status = x.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                    Remarks = x.Remarks,
                }).ToListAsync(ct);
            
            return Result<List<AppointmentRespModel>>.Success(result, "Appointments retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<List<AppointmentRespModel>>.SystemError($"Error retrieving Appointments: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentRespModel>> GetAppointmentById(GetAppointmentByIdQuery request, CancellationToken ct)
    {
        var result = await _db.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

        if (result is null)
        {
            return Result<AppointmentRespModel>.NotFound("Appointment not found");
        }

        var data = new AppointmentRespModel()
        {
            Id = result.Id,
            UserId = result.UserId,
            HospitalId = result.HospitalId,
            DonationId = result.DonationId,
            BloodRequestId = result.BloodRequestId,
            AppointmentDate = result.AppointmentDate,
            AppointmentTime = result.AppointmentTime,
            Status = result.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
            Remarks = result.Remarks,
        };
        
        return Result<AppointmentRespModel>.Success(data, "Appointment retrieved successfully.");
    }

    public async Task<Result<AppointmentRespModel>> CreateDonationAppointment(CreateDonationAppointmentCommand request, CancellationToken ct)
    {
        try
        {
            var donation = await _db.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.DonationId && x.DeletedAt == null, ct);
            
            if (donation is null) 
                return Result<AppointmentRespModel>.NotFound("Donation not found");
            
            var donor = await _db.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == donation.DonorId && x.DeletedAt == null && x.IsActive, ct);

            if (donor is null)
                return Result<AppointmentRespModel>.ValidationError("Donation donor is invalid");
            
            if (!string.Equals(donation.Status, "approved", StringComparison.OrdinalIgnoreCase))
                return Result<AppointmentRespModel>.ValidationError("Donation is not approved");

            if (!donation.DonationDate.HasValue)
                return Result<AppointmentRespModel>.ValidationError("Donation date is required before creating appointment");

            var hasOpenAppointment = await _db.Appointments
                .AnyAsync(x =>
                    x.DonationId == request.DonationId &&
                    x.DeletedAt == null &&
                    x.Status.ToLower() != EnumAppointmentStatus.Cancelled.ToString().ToLower(), ct);

            if (hasOpenAppointment)
                return Result<AppointmentRespModel>.ValidationError("An active appointment already exists for this donation");

            var appointment = new AppointmentEntity()
            {
                UserId = donor.UserId,
                HospitalId = donation.HospitalId,
                DonationId = request.DonationId,
                BloodRequestId = null,
                AppointmentDate = donation.DonationDate.Value,
                AppointmentTime = new TimeOnly(9, 0),
                Status = EnumAppointmentStatus.Scheduled.ToString().ToLowerInvariant(),
                Remarks = request.Remarks,
            };
            
            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync(ct);

            var response = new AppointmentRespModel()
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                HospitalId = appointment.HospitalId,
                DonationId = appointment.DonationId,
                BloodRequestId = appointment.BloodRequestId,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = appointment.Remarks
            };
            
            return Result<AppointmentRespModel>.Success(response, "Donation appointment created successfully.");
        }
        catch (Exception ex)
        {
            return Result<AppointmentRespModel>.SystemError($"Error creating appointment: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentRespModel>> UpdateAppointmentStatus(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        try
        {
            var existingAppointment = await _db.Appointments
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);
            
            if (existingAppointment is null)
                return Result<AppointmentRespModel>.NotFound("Appointment not found");

            if (request.Status == EnumAppointmentStatus.None)
                return Result<AppointmentRespModel>.ValidationError("Invalid appointment status");

            var currentStatus = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None);
            if (currentStatus == EnumAppointmentStatus.None)
                return Result<AppointmentRespModel>.ValidationError("Current appointment status is invalid");

            if (currentStatus == request.Status)
                return Result<AppointmentRespModel>.Success(new AppointmentRespModel
                {
                    Id = existingAppointment.Id,
                    UserId = existingAppointment.UserId,
                    HospitalId = existingAppointment.HospitalId,
                    DonationId = existingAppointment.DonationId,
                    BloodRequestId = existingAppointment.BloodRequestId,
                    AppointmentDate = existingAppointment.AppointmentDate,
                    AppointmentTime = existingAppointment.AppointmentTime,
                    Status = currentStatus,
                    Remarks = existingAppointment.Remarks
                }, "Appointment status is already up to date.");

            bool isAllowedTransition =
                (currentStatus == EnumAppointmentStatus.Scheduled &&
                 (request.Status == EnumAppointmentStatus.Confirmed || request.Status == EnumAppointmentStatus.Cancelled)) ||
                (currentStatus == EnumAppointmentStatus.Confirmed &&
                 request.Status == EnumAppointmentStatus.Cancelled);

            if (!isAllowedTransition)
            {
                return Result<AppointmentRespModel>.ValidationError(
                    $"Invalid status transition: {currentStatus} -> {request.Status}");
            }

            existingAppointment.Status = request.Status.ToString().ToLowerInvariant();
            
            await _db.SaveChangesAsync(ct);
    
            var result = new AppointmentRespModel()
            {
                Id = existingAppointment.Id,
                UserId = existingAppointment.UserId,
                HospitalId = existingAppointment.HospitalId,
                DonationId = existingAppointment.DonationId,
                BloodRequestId = existingAppointment.BloodRequestId,
                AppointmentDate = existingAppointment.AppointmentDate,
                AppointmentTime = existingAppointment.AppointmentTime,
                Status = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = existingAppointment.Remarks,
            };
            
            return Result<AppointmentRespModel>.Success(result, "Appointment status updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<AppointmentRespModel>.SystemError($"Error updating appointment status: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentRespModel>> CompleteAppointment(CompleteAppointmentCommand request, CancellationToken ct)
    {
        try
        {
            var existingAppointment = await _db.Appointments
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

            if (existingAppointment is null)
                return Result<AppointmentRespModel>.NotFound("Appointment not found");

            var currentStatus = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None);
            if (currentStatus != EnumAppointmentStatus.Confirmed)
                return Result<AppointmentRespModel>.ValidationError("Only confirmed appointments can be completed");

            existingAppointment.Status = EnumAppointmentStatus.Completed.ToString().ToLowerInvariant();

            if (existingAppointment.BloodRequestId.HasValue)
            {
                var bloodRequest = await _db.BloodRequests
                    .FirstOrDefaultAsync(x => x.Id == existingAppointment.BloodRequestId.Value && x.DeletedAt == null, ct);
                
                if (bloodRequest is null)
                    return Result<AppointmentRespModel>.NotFound("Blood request not found");
                
                var inventories = await _db.BloodInventories
                    .Where(x => x.DeletedAt == null &&
                                x.Status == "available" &&
                                x.HospitalId == existingAppointment.HospitalId && 
                                x.BloodGroup == bloodRequest.BloodGroup)
                    .OrderBy(x => x.ExpiredAt)
                    .Take(bloodRequest.UnitsRequired)
                    .ToListAsync(ct);

                var totalAvailableUnits = inventories.Sum(x => x.Units);
                
                if (totalAvailableUnits < bloodRequest.UnitsRequired)
                    return Result<AppointmentRespModel>.ValidationError(
                        $"Insufficient stock. Available: {totalAvailableUnits}, Required: {bloodRequest.UnitsRequired}");
                
                var remainingUnits = bloodRequest.UnitsRequired;

                foreach (var item in inventories)
                {
                    if (remainingUnits <= 0) break;

                    if (item.Units <= remainingUnits)
                    {
                        remainingUnits -= item.Units;
                        item.Units = 0;
                        item.Status = "used";
                        item.RequestId = bloodRequest.Id;
                    }
                    else
                    {
                        item.Units -= remainingUnits;
                        remainingUnits = 0;
                    }

                    item.UpdatedAt = DateTime.UtcNow;
                }

                bloodRequest.Status = EnumBloodRequestStatus.Fulfilled.ToDatabaseValue();
                bloodRequest.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);

            var response = new AppointmentRespModel()
            {
                Id = existingAppointment.Id,
                UserId = existingAppointment.UserId,
                HospitalId = existingAppointment.HospitalId,
                DonationId = existingAppointment.DonationId,
                BloodRequestId = existingAppointment.BloodRequestId,
                AppointmentDate = existingAppointment.AppointmentDate,
                AppointmentTime = existingAppointment.AppointmentTime,
                Status = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = existingAppointment.Remarks,
            };

            var message = existingAppointment.BloodRequestId.HasValue
                ? "Appointment completed and blood request fulfilled successfully."
                : "Appointment completed successfully.";

            return Result<AppointmentRespModel>.Success(response, message);
        }
        catch (Exception ex)
        {
            return Result<AppointmentRespModel>.SystemError($"Error completing appointment: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteAppointment(DeleteAppointmentCommand request, CancellationToken ct)
    {
        try
        {
            var existingAppointment = await _db.Appointments
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct);

            if (existingAppointment is null)
                return Result<string>.NotFound("Appointment not found");

            existingAppointment.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Result<string>.DeleteSuccess("Appointment deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"Error deleting appointment: {ex.Message}");
        }
    }
}
