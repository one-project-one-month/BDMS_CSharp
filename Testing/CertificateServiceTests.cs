using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Certificate;
using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Testing;

public class CertificateServiceTests
{
    [Fact]
    public async Task GenerateCertificate_WithDonationRecord_ReturnsSuccess()
    {
        using var db = CreateDbContext();
        SeedDonorWithUser(db, donorId: 1, userId: 101);
        db.Donations.Add(new Donation
        {
            Id = 1,
            DonorId = 1,
            HospitalId = 1,
            CreatedBy = 101,
            BloodGroup = "A+",
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CertificateService(db);
        var command = new GenerateCertificateCommand
        {
            Certificate = new CertificateReqModel { DonorId = 1, CertificateTitle = "Test" }
        };

        var result = await service.GenerateCertificate(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(101, result.Data!.UserId);
    }

    [Fact]
    public async Task GenerateCertificate_WithoutDonationRecord_ReturnsValidationError()
    {
        using var db = CreateDbContext();
        SeedDonorWithUser(db, donorId: 2, userId: 202);
        await db.SaveChangesAsync();

        var service = new CertificateService(db);
        var command = new GenerateCertificateCommand
        {
            Certificate = new CertificateReqModel { DonorId = 2, CertificateTitle = "Test" }
        };

        var result = await service.GenerateCertificate(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Donor must have at least one donation record to generate a certificate", result.Message);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedDonorWithUser(AppDbContext db, int donorId, int userId)
    {
        db.Users.Add(new User
        {
            Id = userId,
            UserName = $"user-{userId}",
            Email = $"user{userId}@test.com",
            Password = "hash",
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.Donors.Add(new Donor
        {
            Id = donorId,
            UserId = userId,
            NicNo = $"NIC-{donorId}",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Gender = "Male",
            BloodGroup = "A+",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
}
