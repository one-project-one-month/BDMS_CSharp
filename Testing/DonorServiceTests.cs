using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Donor.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Testing;

public class DonorServiceTests
{
    [Fact]
    public async Task BecomeDonor_CreatesPendingInactiveDonorWithoutRoleChange()
    {
        await using var db = CreateDbContext();
        var (userRole, donorRole) = SeedRoles(db);
        var user = SeedUser(db, userRole.Id, "somchai");

        var service = new DonorService(db);
        var result = await service.BecomeDonor(user.Id, BuildReq(user.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("pending", result.Data!.Status);
        Assert.False(result.Data.IsActive);

        var saved = await db.Donors.SingleAsync(d => d.UserId == user.Id);
        Assert.Equal("pending", saved.Status);
        Assert.False(saved.IsActive);

        var reloadedUser = await db.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal(userRole.Id, reloadedUser.RoleId);
        Assert.NotEqual(donorRole.Id, reloadedUser.RoleId);
    }

    [Fact]
    public async Task BecomeDonor_RejectsSecondApplicationForSameUser()
    {
        await using var db = CreateDbContext();
        var (userRole, _) = SeedRoles(db);
        var user = SeedUser(db, userRole.Id, "somchai");

        var service = new DonorService(db);
        var first = await service.BecomeDonor(user.Id, BuildReq(user.Id));
        var second = await service.BecomeDonor(user.Id, BuildReq(user.Id));

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(1, await db.Donors.CountAsync(d => d.UserId == user.Id));
    }

    [Fact]
    public async Task UpdateDonorStatus_ActivateApprovesPendingDonorAndAssignsDonorRole()
    {
        await using var db = CreateDbContext();
        var (userRole, donorRole) = SeedRoles(db);
        var user = SeedUser(db, userRole.Id, "somchai");

        var service = new DonorService(db);
        await service.BecomeDonor(user.Id, BuildReq(user.Id));
        var donorId = (await db.Donors.SingleAsync(d => d.UserId == user.Id)).Id;

        var result = await service.UpdateDonorStatus(donorId, isActive: true);

        Assert.True(result.IsSuccess);
        Assert.Equal("approved", result.Data!.Status);
        Assert.True(result.Data.IsActive);

        var reloadedUser = await db.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal(donorRole.Id, reloadedUser.RoleId);
    }

    private static DonorReqModel BuildReq(int userId) => new()
    {
        UserId = userId,
        NicNo = "9/NRC111111",
        DateOfBirth = new DateOnly(1995, 1, 12),
        Gender = "male",
        BloodGroup = "A+",
        EmergencyContact = "Jane",
        EmergencyPhone = "0991234567",
        Address = "Yangon",
        IsActive = false
    };

    private static (Role userRole, Role donorRole) SeedRoles(AppDbContext db)
    {
        var userRole = new Role { Id = 1, Name = "user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var donorRole = new Role { Id = 2, Name = "donor", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Roles.AddRange(userRole, donorRole);
        db.SaveChanges();
        return (userRole, donorRole);
    }

    private static User SeedUser(AppDbContext db, int roleId, string userName)
    {
        var user = new User
        {
            Id = 10,
            RoleId = roleId,
            UserName = userName,
            Email = $"{userName}@test.com",
            Password = "x",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
