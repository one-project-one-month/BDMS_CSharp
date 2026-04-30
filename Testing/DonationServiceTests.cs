using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.BloodInventory;
using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donations.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Testing;

public class DonationServiceTests
{
    [Fact]
    public async Task CreateDonation_GeneratesNextDonationCodeSeries()
    {
        await using var db = CreateDbContext();
        SeedHospital(db, hospitalId: 2, name: "City Hospital");

        db.Donations.Add(new Donation
        {
            Id = 1,
            DonorId = 10,
            HospitalId = 2,
            BloodRequestId = 1,
            CreatedBy = 99,
            DonationCode = "CITYHOSPITAL_A+_01",
            BloodGroup = "A+",
            UnitsDonated = 1,
            DonationDate = new DateOnly(2026, 1, 1),
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.CreateDonation(new DonationCreateReqModel
        {
            DonorId = 11,
            HospitalId = 2,
            BloodRequestId = 1,
            CreatedBy = 100,
            DonationCode = "IGNORED",
            BloodGroup = "A+",
            UnitsDonated = 1,
            DonationDate = new DateOnly(2026, 1, 2),
            Remarks = "new"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("CITYHOSPITAL_A+_02", result.Data!.DonationCode);
    }

    private static DonationService CreateService(AppDbContext db)
    {
        var inventoryService = new Mock<IBloodInventoryService>();
        return new DonationService(db, inventoryService.Object);
    }

    private static AppDbContext CreateDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedHospital(AppDbContext db, int hospitalId, string name)
    {
        db.Hospitals.Add(new Hospital
        {
            Id = hospitalId,
            Name = name,
            Phone = "0100000000",
            Address = "Main road",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}
