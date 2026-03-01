using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<BloodInventory> BloodInventories { get; set; }

    public virtual DbSet<BloodRequest> BloodRequests { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Donor> Donors { get; set; }

    public virtual DbSet<Hospital> Hospitals { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Announce__3213E83FD3CB9D04");

            entity.ToTable(tb => tb.HasTrigger("trg_Announcements_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3213E83F735F62EA");

            entity.ToTable(tb => tb.HasTrigger("trg_Appointments_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
            entity.Property(e => e.AppointmentTime).HasColumnName("appointment_time");
            entity.Property(e => e.BloodRequestId).HasColumnName("blood_request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DonationId).HasColumnName("donation_id");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("scheduled")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.BloodRequestId)
                .HasConstraintName("FK_Appointments_BloodRequest");

            entity.HasOne(d => d.Donation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DonationId)
                .HasConstraintName("FK_Appointments_Donation");

            entity.HasOne(d => d.Hospital).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Hospital");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_User");
        });

        modelBuilder.Entity<BloodInventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Blood_In__3213E83FC4FB5E51");

            entity.ToTable("Blood_Inventories", tb => tb.HasTrigger("trg_BloodInventories_UpdatedAt"));

            entity.HasIndex(e => e.DonationId, "UQ_BloodInventories_Donation").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .HasColumnName("blood_group");
            entity.Property(e => e.CollectedAt).HasColumnName("collected_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DonationId).HasColumnName("donation_id");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .HasDefaultValue("available")
                .HasColumnName("status");
            entity.Property(e => e.Units).HasColumnName("units");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Donation).WithOne(p => p.BloodInventory)
                .HasForeignKey<BloodInventory>(d => d.DonationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodInventories_Donation");

            entity.HasOne(d => d.Hospital).WithMany(p => p.BloodInventories)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodInventories_Hospital");

            entity.HasOne(d => d.Request).WithMany(p => p.BloodInventories)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK_BloodInventories_Request");
        });

        modelBuilder.Entity<BloodRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Blood_Re__3213E83F33FFC6F7");

            entity.ToTable("Blood_Requests", tb => tb.HasTrigger("trg_BloodRequests_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .HasColumnName("blood_group");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(20)
                .HasColumnName("contact_phone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.PatientName)
                .HasMaxLength(255)
                .HasColumnName("patient_name");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RequiredDate).HasColumnName("required_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UnitsRequired)
                .HasDefaultValue(1)
                .HasColumnName("units_required");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Urgency)
                .HasMaxLength(10)
                .HasDefaultValue("low")
                .HasColumnName("urgency");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.BloodRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_BloodRequests_ApprovedBy");

            entity.HasOne(d => d.Hospital).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodRequests_Hospital");

            entity.HasOne(d => d.User).WithMany(p => p.BloodRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodRequests_User");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Certific__3213E83F89FF4A90");

            entity.ToTable(tb => tb.HasTrigger("trg_Certificates_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateData).HasColumnName("certificate_data");
            entity.Property(e => e.CertificateDescription)
                .HasMaxLength(500)
                .HasColumnName("certificate_description");
            entity.Property(e => e.CertificateTitle)
                .HasMaxLength(255)
                .HasColumnName("certificate_title");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_User");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3213E83F50F0DA08");

            entity.ToTable(tb => tb.HasTrigger("trg_Donations_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .HasColumnName("blood_group");
            entity.Property(e => e.BloodRequestId).HasColumnName("blood_request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DonationDate).HasColumnName("donation_date");
            entity.Property(e => e.DonorId).HasColumnName("donor_id");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UnitsDonated).HasColumnName("units_donated");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.DonationApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_Donations_ApprovedBy");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.Donations)
                .HasForeignKey(d => d.BloodRequestId)
                .HasConstraintName("FK_Donations_BloodRequest");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DonationCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_CreatedBy");

            entity.HasOne(d => d.Donor).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_Donor");

            entity.HasOne(d => d.Hospital).WithMany(p => p.Donations)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_Hospital");
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donors__3213E83F708B4744");

            entity.ToTable(tb => tb.HasTrigger("trg_Donors_UpdatedAt"));

            entity.HasIndex(e => e.NicNo, "UQ_Donors_NicNo").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_Donors_UserId").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .HasColumnName("blood_group");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(255)
                .HasColumnName("emergency_contact");
            entity.Property(e => e.EmergencyPhone)
                .HasMaxLength(20)
                .HasColumnName("emergency_phone");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastDonationDate).HasColumnName("last_donation_date");
            entity.Property(e => e.NicNo)
                .HasMaxLength(50)
                .HasColumnName("nic_no");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Donor)
                .HasForeignKey<Donor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donors_User");
        });

        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hospital__3213E83F25B9463A");

            entity.ToTable(tb => tb.HasTrigger("trg_Hospitals_UpdatedAt"));

            entity.HasIndex(e => e.Email, "UQ_Hospitals_Email").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ_Hospitals_Phone").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3213E83F1D75B86C");

            entity.ToTable("Medical_Records", tb => tb.HasTrigger("trg_MedicalRecords_UpdatedAt"));

            entity.HasIndex(e => e.DonationId, "UQ_MedicalRecords_Donation").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DonationId).HasColumnName("donation_id");
            entity.Property(e => e.HemoglobinLevel)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("hemoglobin_level");
            entity.Property(e => e.HepatitisBResult)
                .HasMaxLength(15)
                .HasColumnName("hepatitis_b_result");
            entity.Property(e => e.HepatitisCResult)
                .HasMaxLength(15)
                .HasColumnName("hepatitis_c_result");
            entity.Property(e => e.HivResult)
                .HasMaxLength(15)
                .HasColumnName("hiv_result");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.MalariaResult)
                .HasMaxLength(15)
                .HasColumnName("malaria_result");
            entity.Property(e => e.ScreenedBy).HasColumnName("screened_by");
            entity.Property(e => e.ScreeningAt).HasColumnName("screening_at");
            entity.Property(e => e.ScreeningNotes).HasColumnName("screening_notes");
            entity.Property(e => e.ScreeningStatus)
                .HasMaxLength(10)
                .HasColumnName("screening_status");
            entity.Property(e => e.SyphilisResult)
                .HasMaxLength(15)
                .HasColumnName("syphilis_result");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Donation).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.DonationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Donation");

            entity.HasOne(d => d.Hospital).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Hospital");

            entity.HasOne(d => d.ScreenedByNavigation).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.ScreenedBy)
                .HasConstraintName("FK_MedicalRecords_ScreenedBy");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3213E83F187A6861");

            entity.ToTable(tb => tb.HasTrigger("trg_Permissions_UpdatedAt"));

            entity.HasIndex(e => e.Name, "UQ_Permissions_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3213E83F63587BBF");

            entity.ToTable(tb => tb.HasTrigger("trg_Roles_UpdatedAt"));

            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId }).HasName("PK_RolePermissions");

            entity.ToTable("Role_Permissions", tb => tb.HasTrigger("trg_RolePermissions_UpdatedAt"));

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermissions_Perm");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermissions_Role");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83FC4BF364B");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_UpdatedAt"));

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.HospitalId).HasColumnName("hospital_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");

            entity.HasOne(d => d.Hospital).WithMany(p => p.Users)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK_Users_Hospital");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
