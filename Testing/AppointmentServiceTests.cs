using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Appointment;
using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Testing;

public class AppointmentServiceTests
{
    [Fact]
    public async Task CompleteAppointment_ForDonation_CreatesMedicalRecordWithDonationHospital()
    {
        await using var db = CreateDbContext();

        var donation = new Donation
        {
            Id = 1,
            DonorId = 5,
            HospitalId = 9,
            CreatedBy = 1,
            BloodGroup = "A+",
            Status = "approved",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Donations.Add(donation);
        db.Appointments.Add(new Appointment
        {
            Id = 1,
            UserId = 10,
            HospitalId = 9,
            DonationId = 1,
            AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AppointmentTime = new TimeOnly(10, 0),
            Status = EnumAppointmentStatus.Confirmed.ToString().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new AppointmentService(db);
        var result = await service.CompleteAppointment(new CompleteAppointmentCommand { Id = 1 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CompleteAppointment_UpdatesExistingMedicalRecordHospitalId()
    {
        await using var db = CreateDbContext();

        db.Donations.Add(new Donation
        {
            Id = 2,
            DonorId = 8,
            HospitalId = 11,
            CreatedBy = 1,
            BloodGroup = "O+",
            Status = "approved",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.Appointments.Add(new Appointment
        {
            Id = 2,
            UserId = 10,
            HospitalId = 3,
            DonationId = 2,
            AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AppointmentTime = new TimeOnly(9, 30),
            Status = EnumAppointmentStatus.Confirmed.ToString().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.MedicalRecords.Add(new MedicalRecord
        {
            Id = 20,
            DonationId = 2,
            HospitalId = 3,
            ScreeningStatus = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new AppointmentService(db);
        var result = await service.CompleteAppointment(new CompleteAppointmentCommand { Id = 2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CompleteAppointment_WhenInventoryAlreadyExists_CompletesWithoutDuplicateInventory()
    {
        await using var db = CreateDbContext();

        db.Donations.Add(new Donation
        {
            Id = 3,
            DonorId = 12,
            HospitalId = 5,
            CreatedBy = 1,
            BloodGroup = "O+",
            UnitsDonated = 2,
            DonationDate = new DateOnly(2026, 11, 1),
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.BloodInventories.Add(new BloodInventory
        {
            Id = 30,
            DonationId = 3,
            HospitalId = 5,
            BloodGroup = "O+",
            Units = 2,
            CollectedAt = new DateOnly(2026, 11, 1),
            ExpiredAt = new DateOnly(2026, 12, 13),
            Status = "available",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.Appointments.Add(new Appointment
        {
            Id = 3,
            UserId = 10,
            HospitalId = 5,
            DonationId = 3,
            AppointmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AppointmentTime = new TimeOnly(9, 0),
            Status = EnumAppointmentStatus.Confirmed.ToString().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new AppointmentService(db);
        var result = await service.CompleteAppointment(new CompleteAppointmentCommand { Id = 3 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("already existed", result.Message);

        var appointment = await db.Appointments.SingleAsync(x => x.Id == 3);
        Assert.Equal("completed", appointment.Status);
        Assert.Equal(1, await db.BloodInventories.CountAsync(x => x.DonationId == 3));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
