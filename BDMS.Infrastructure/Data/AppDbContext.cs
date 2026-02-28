using BDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BloodDonor> BloodDonors => Set<BloodDonor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<BloodDonor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.BloodType).IsRequired().HasMaxLength(10);
        });

        base.OnModelCreating(modelBuilder);
    }
}
