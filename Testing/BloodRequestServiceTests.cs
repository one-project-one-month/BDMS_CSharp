using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodInventory;
using BDMS.Domain.Features.BloodRequest;
using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Testing;

public class BloodRequestServiceTests
{
    [Fact]
    public async Task Create_GeneratesNextBloodRequestCodeSeries()
    {
        await using var db = CreateDbContext();
        SeedHospital(db, hospitalId: 2, name: "City Hospital");

        db.BloodRequests.Add(new BloodRequest
        {
            Id = 1,
            UserId = 10,
            HospitalId = 2,
            BloodRequestCode = "CITYHOSPITAL_A+_01",
            PatientName = "Seed Patient",
            BloodGroup = "A+",
            UnitsRequired = 1,
            Urgency = "high",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.Create(new CreateBloodRequestCommand
        {
            UserId = 11,
            HospitalId = 2,
            PatientName = "New Patient",
            BloodGroup = "A+",
            UnitsRequired = 2,
            Urgency = EnumBloodRequestUrgency.High,
            RequiredDate = new DateOnly(2026, 3, 25),
            Reason = "Emergency"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("CITYHOSPITAL_A+_02", result.Data!.BloodRequestCode);
    }

    [Fact]
    public async Task Create_WhenCalledConcurrently_GeneratesUniqueSequentialCodes()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var setupDb = CreateDbContext(dbName))
        {
            SeedHospital(setupDb, hospitalId: 2, name: "City Hospital");
        }

        async Task<string?> CreateRequestAsync(int userId, string patientName)
        {
            await using var db = CreateDbContext(dbName);
            var service = CreateService(db);
            var result = await service.Create(new CreateBloodRequestCommand
            {
                UserId = userId,
                HospitalId = 2,
                PatientName = patientName,
                BloodGroup = "A+",
                UnitsRequired = 1,
                Urgency = EnumBloodRequestUrgency.High,
                RequiredDate = new DateOnly(2026, 3, 25),
                Reason = "Concurrent creation test"
            }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            return result.Data?.BloodRequestCode;
        }

        var createdCodes = await Task.WhenAll(
            CreateRequestAsync(100, "Concurrent Patient 1"),
            CreateRequestAsync(101, "Concurrent Patient 2"));

        Assert.Equal(2, createdCodes.Distinct().Count());
        Assert.Contains(createdCodes, code => code!.EndsWith("_01"));
        Assert.Contains(createdCodes, code => code!.EndsWith("_02"));
    }

    [Fact]
    public async Task Update_KeepsBloodRequestCodeImmutable()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 2, 10), code: "CITYHOSPITAL_A+_01");

        var service = CreateService(db);
        var result = await service.Update(new UpdateBloodRequestCommand
        {
            Id = 1,
            UserId = 99,
            HospitalId = 2,
            PatientName = "Updated Patient",
            BloodGroup = "A+",
            UnitsRequired = 3,
            ContactPhone = "0111111111",
            Urgency = EnumBloodRequestUrgency.Medium,
            RequiredDate = new DateOnly(2026, 2, 12),
            Reason = "Updated reason"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("CITYHOSPITAL_A+_01", result.Data!.BloodRequestCode);

        var saved = await db.BloodRequests.SingleAsync(x => x.Id == 1);
        Assert.Equal("CITYHOSPITAL_A+_01", saved.BloodRequestCode);
    }

    //[Fact]
    //public async Task Update_NonPendingRequest_ReturnsValidationError()
    //{
    //    await using var db = CreateDbContext();
    //    SeedBloodRequest(db, status: "approved", requiredDate: new DateOnly(2026, 2, 10));

    //    var service = CreateService(db);
    //    var result = await service.Update(new UpdateBloodRequestCommand
    //    {
    //        Id = 1,
    //        UserId = 10,
    //        HospitalId = 2,
    //        PatientName = "Updated Patient",
    //        BloodGroup = "A+",
    //        UnitsRequired = 2,
    //        ContactPhone = "012345678",
    //        Urgency = EnumBloodRequestUrgency.High,
    //        RequiredDate = new DateOnly(2026, 2, 12),
    //        Reason = "Updated reason"
    //    }, CancellationToken.None);

    //    Assert.False(result.IsSuccess);
    //    Assert.Contains("Only pending blood requests can be updated", result.Message);
    //}

    [Fact]
    public async Task UpdateStatus_ApproveWithoutRequiredDate_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: null);
        SeedDonor(db, donorId: 7, userId: 100, bloodGroup: "A+");

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Approved,
            DonorId = 7
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("RequiredDate is required when approving a blood request.", result.Message);
        Assert.Empty(db.Appointments);
    }

    //[Fact]
    //public async Task UpdateStatus_Approve_CreatesScheduledAppointment()
    //{
    //    await using var db = CreateDbContext();
    //    SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 3, 20));
    //    SeedDonor(db, donorId: 7, userId: 100, bloodGroup: "A+");

    //    var service = CreateService(db);
    //    var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
    //    {
    //        Id = 1,
    //        Status = EnumBloodRequestStatus.Approved,
    //        DonorId = 7
    //    }, CancellationToken.None);

    //    Assert.True(result.IsSuccess);

    //    var appointment = await db.Appointments.SingleAsync();
    //    Assert.Equal(1, appointment.BloodRequestId);
    //    Assert.Equal("scheduled", appointment.Status);
    //    Assert.Equal(new DateOnly(2026, 3, 20), appointment.AppointmentDate);

    //    var request = await db.BloodRequests.SingleAsync(x => x.Id == 1);
    //    Assert.Equal(100, request.ApprovedBy);
    //    Assert.NotNull(request.ApprovedAt);
    //}

    [Fact]
    public async Task UpdateStatus_Approve_WithCancelledAppointmentDifferentCasing_CreatesNewAppointment()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 3, 20));
        SeedDonor(db, donorId: 7, userId: 100, bloodGroup: "A+");
        SeedAppointment(db, bloodRequestId: 1, status: "CANCELLED");

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Approved,
            DonorId = 7
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, await db.Appointments.CountAsync());
        Assert.Equal(1, await db.Appointments.CountAsync(x => x.Status == "scheduled"));
    }

    [Fact]
    public async Task UpdateStatus_Approve_WithoutDonorId_DoesNotRequireDonor()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 3, 20));

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Approved,
            DonorId = null
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var request = await db.BloodRequests.SingleAsync(x => x.Id == 1);
        Assert.Null(request.ApprovedBy);
        Assert.Null(request.ApprovedAt);

        var appointment = await db.Appointments.SingleAsync();
        Assert.Equal("scheduled", appointment.Status);
    }

    [Fact]
    public async Task UpdateStatus_Fulfilled_WithoutDonorId_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "approved", requiredDate: new DateOnly(2026, 3, 20));

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Fulfilled,
            DonorId = null
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("DonorId is required when fulfilling a blood request.", result.Message);
    }

    [Fact]
    public async Task UpdateStatus_WithMismatchedDonorBloodGroup_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 3, 20));
        SeedDonor(db, donorId: 8, userId: 101, bloodGroup: "B+");

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Fulfilled,
            DonorId = 8
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Donor blood group does not match request blood group.", result.Message);
        Assert.Empty(db.Appointments);
    }


    [Fact]
    public async Task UpdateStatus_WithEqualDonorBloodGroup_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        SeedBloodRequest(db, status: "pending", requiredDate: new DateOnly(2026, 3, 20));
        SeedDonor(db, donorId: 8, userId: 101, bloodGroup: "A+");

        var service = CreateService(db);
        var result = await service.UpdateStatus(new UpdateBloodRequestStatusCommand
        {
            Id = 1,
            Status = EnumBloodRequestStatus.Approved,
            DonorId = 8
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private static BloodRequestService CreateService(AppDbContext db)
    {
        var bloodInventoryService = new Mock<IBloodInventoryService>();
        return new BloodRequestService(db);
    }

    private static AppDbContext CreateDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedBloodRequest(AppDbContext db, string status, DateOnly? requiredDate, string? code = null)
    {
        db.BloodRequests.Add(new BloodRequest
        {
            Id = 1,
            UserId = 10,
            HospitalId = 2,
            BloodRequestCode = code,
            PatientName = "John Doe",
            BloodGroup = "A+",
            UnitsRequired = 2,
            Urgency = "high",
            RequiredDate = requiredDate,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    private static void SeedHospital(AppDbContext db, int hospitalId, string name)
    {
        db.Hospitals.Add(new Hospital
        {
            Id = hospitalId,
            Name = name,
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    private static void SeedDonor(AppDbContext db, int donorId, int userId, string bloodGroup)
    {
        db.Donors.Add(new Donor
        {
            Id = donorId,
            UserId = userId,
            NicNo = "123456789V",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = "male",
            BloodGroup = bloodGroup,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    private static void SeedAppointment(AppDbContext db, int bloodRequestId, string status)
    {
        db.Appointments.Add(new Appointment
        {
            Id = 100,
            UserId = 10,
            HospitalId = 2,
            BloodRequestId = bloodRequestId,
            AppointmentDate = new DateOnly(2026, 3, 19),
            AppointmentTime = new TimeOnly(8, 30),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}
