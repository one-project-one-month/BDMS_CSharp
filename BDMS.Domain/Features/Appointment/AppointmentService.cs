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

    public async Task<Result<List<AppointmentRespModel>>> GetAllAppointments(CancellationToken ct)
    {
        try
        {
            var result = await _db.Appointments
                .Where(x => x.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync(ct);

            var data = result.Select(x => new AppointmentRespModel()
            {
                UserId = x.UserId,
                HospitalId = x.HospitalId,
                DonationId = x.DonationId,
                BloodRequestId = x.BloodRequestId,
                AppointmentDate = x.AppointmentDate,
                AppointmentTime = x.AppointmentTime,
                Status = x.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = x.Remarks,
            }).ToList();
            
            return Result<List<AppointmentRespModel>>.Success(data, "Success");
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
            UserId = result.UserId,
            HospitalId = result.HospitalId,
            DonationId = result.DonationId,
            BloodRequestId = result.BloodRequestId,
            AppointmentDate = result.AppointmentDate,
            AppointmentTime = result.AppointmentTime,
            Status = result.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
            Remarks = result.Remarks,
        };
        
        return Result<AppointmentRespModel>.Success(data, "Success");
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
                UserId = appointment.UserId,
                HospitalId = appointment.HospitalId,
                DonationId = appointment.DonationId,
                BloodRequestId = appointment.BloodRequestId,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = appointment.Remarks
            };
            
            return Result<AppointmentRespModel>.Success(response, "Appointment created");
        }
        catch (Exception ex)
        {
            return Result<AppointmentRespModel>.SystemError($"Error creating appointment: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentRespModel>> CreateBloodRequestAppointment(CreateBloodRequestAppointmentCommand request, CancellationToken ct)
    {
        try
        {
            var bloodRequest = await _db.BloodRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BloodRequestId && x.DeletedAt == null, ct);

            if (bloodRequest is null)
            {
                return Result<AppointmentRespModel>.NotFound("Blood request not found");
            }
            
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == bloodRequest.UserId && x.DeletedAt == null && x.IsActive, ct);
            
            if (user is null)
                return Result<AppointmentRespModel>.ValidationError("Request user is invalid");
            
            if (!string.Equals(bloodRequest.Status, "approved", StringComparison.OrdinalIgnoreCase))
                return Result<AppointmentRespModel>.ValidationError("Request is not approved");

            if (!bloodRequest.RequiredDate.HasValue)
                return Result<AppointmentRespModel>.ValidationError("Required date is missing for this blood request");

            var hasOpenAppointment = await _db.Appointments
                .AnyAsync(x =>
                    x.BloodRequestId == request.BloodRequestId &&
                    x.DeletedAt == null &&
                    x.Status.ToLower() != EnumAppointmentStatus.Cancelled.ToString().ToLower(), ct);
            
            if (hasOpenAppointment) 
                return Result<AppointmentRespModel>.ValidationError("An active appointment already exists for this request");

            var appointment = new AppointmentEntity()
            {
                UserId = user.Id,
                HospitalId = bloodRequest.HospitalId,
                BloodRequestId = request.BloodRequestId,
                AppointmentDate = bloodRequest.RequiredDate.Value,
                AppointmentTime = new TimeOnly(9, 0),
                Status = EnumAppointmentStatus.Scheduled.ToString().ToLowerInvariant(),
                Remarks = request.Remarks,
            };
            
            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync(ct);

            var response = new AppointmentRespModel()
            {
                UserId = appointment.UserId,
                HospitalId = appointment.HospitalId,
                DonationId = appointment.DonationId,
                BloodRequestId = appointment.BloodRequestId,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = appointment.Remarks,
            };
            
            return Result<AppointmentRespModel>.Success(response, "Appointment created");
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
                    UserId = existingAppointment.UserId,
                    HospitalId = existingAppointment.HospitalId,
                    DonationId = existingAppointment.DonationId,
                    BloodRequestId = existingAppointment.BloodRequestId,
                    AppointmentDate = existingAppointment.AppointmentDate,
                    AppointmentTime = existingAppointment.AppointmentTime,
                    Status = currentStatus,
                    Remarks = existingAppointment.Remarks
                }, "Appointment status unchanged");

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
                UserId = existingAppointment.UserId,
                HospitalId = existingAppointment.HospitalId,
                DonationId = existingAppointment.DonationId,
                BloodRequestId = existingAppointment.BloodRequestId,
                AppointmentDate = existingAppointment.AppointmentDate,
                AppointmentTime = existingAppointment.AppointmentTime,
                Status = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = existingAppointment.Remarks,
            };
            
            return Result<AppointmentRespModel>.Success(result, "Appointment updated");
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
            
            await _db.SaveChangesAsync(ct);

            var response = new AppointmentRespModel()
            {
                UserId = existingAppointment.UserId,
                HospitalId = existingAppointment.HospitalId,
                DonationId = existingAppointment.DonationId,
                BloodRequestId = existingAppointment.BloodRequestId,
                AppointmentDate = existingAppointment.AppointmentDate,
                AppointmentTime = existingAppointment.AppointmentTime,
                Status = existingAppointment.Status.ToEnumOrDefault(EnumAppointmentStatus.None),
                Remarks = existingAppointment.Remarks,
            };
            
            return Result<AppointmentRespModel>.Success(response, "Appointment completed");
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

            return Result<string>.DeleteSuccess("Appointment deleted");
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"Error deleting appointment: {ex.Message}");
        }
    }
}
