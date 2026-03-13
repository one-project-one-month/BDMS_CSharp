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

        var savedDonation = await db.Donations.SingleAsync(x => x.Id == 1);
        Assert.Equal("completed", savedDonation.Status);

        var savedMedicalRecord = await db.MedicalRecords.SingleAsync(x => x.DonationId == 1 && x.DeletedAt == null);
        Assert.Equal(9, savedMedicalRecord.HospitalId);
        Assert.Equal("pending", savedMedicalRecord.ScreeningStatus);
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

        var medicalRecord = await db.MedicalRecords.SingleAsync(x => x.DonationId == 2 && x.DeletedAt == null);
        Assert.Equal(11, medicalRecord.HospitalId);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
