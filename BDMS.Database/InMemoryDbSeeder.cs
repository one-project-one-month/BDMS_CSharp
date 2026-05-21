using BDMS.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDMS.Database;

public static class InMemoryDbSeeder
{
    public static void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // 1. Check if already seeded
        if (context.Roles.Any()) return;

        // 2. Roles
        var adminRole = new Role { Name = "admin", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var staffRole = new Role { Name = "staff", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var donorRole = new Role { Name = "donor", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var userRole = new Role { Name = "user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        
        context.Roles.AddRange(adminRole, staffRole, donorRole, userRole);
        context.SaveChanges();

        // 3. Permissions
        var permissionNames = new[]
        {
            "users.view", "users.create", "users.update", "users.delete",
            "donors.view", "donors.create", "donors.update", "donors.delete",
            "hospitals.view", "hospitals.create", "hospitals.update", "hospitals.delete",
            "blood_requests.view", "blood_requests.create", "blood_requests.update", "blood_requests.approve", "blood_requests.delete",
            "donations.view", "donations.create", "donations.update", "donations.approve", "donations.delete",
            "appointments.view", "appointments.create", "appointments.update", "appointments.delete",
            "medical_records.view", "medical_records.create", "medical_records.update", "medical_records.delete",
            "blood_inventories.view", "blood_inventories.create", "blood_inventories.update", "blood_inventories.delete",
            "announcements.view", "announcements.create", "announcements.update", "announcements.delete",
            "certificates.view", "certificates.create", "certificates.update", "certificates.delete",
            "roles.view", "roles.create", "roles.update", "roles.delete",
            "permissions.view", "permissions.manage"
        };

        var permissions = permissionNames.Select(name => new Permission { Name = name, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }).ToList();
        context.Permissions.AddRange(permissions);
        context.SaveChanges();

        // 4. Role_Permissions
        // Admin gets all
        foreach (var p in permissions)
        {
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }

        // Staff gets operational
        var staffPerms = new[]
        {
            "users.view", "donors.view", "donors.create", "donors.update", "hospitals.view",
            "blood_requests.view", "blood_requests.create", "blood_requests.update", "blood_requests.approve",
            "donations.view", "donations.create", "donations.update", "donations.approve",
            "appointments.view", "appointments.create", "appointments.update",
            "medical_records.view", "medical_records.create", "medical_records.update",
            "blood_inventories.view", "blood_inventories.create", "blood_inventories.update",
            "announcements.view", "certificates.view", "certificates.create"
        };
        foreach (var p in permissions.Where(x => staffPerms.Contains(x.Name)))
        {
            context.RolePermissions.Add(new RolePermission { RoleId = staffRole.Id, PermissionId = p.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }

        // Donor gets self-service
        var donorPerms = new[]
        {
            "donors.view", "donations.view", "donations.create", "donations.update",
            "appointments.view", "medical_records.view", "announcements.view", "certificates.view"
        };
        foreach (var p in permissions.Where(x => donorPerms.Contains(x.Name)))
        {
            context.RolePermissions.Add(new RolePermission { RoleId = donorRole.Id, PermissionId = p.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }

        // User gets read-only/minimal
        var userPerms = new[]
        {
            "blood_requests.view", "blood_requests.create", "announcements.view", "appointments.view", "appointments.create"
        };
        foreach (var p in permissions.Where(x => userPerms.Contains(x.Name)))
        {
            context.RolePermissions.Add(new RolePermission { RoleId = userRole.Id, PermissionId = p.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }
        context.SaveChanges();

        // 5. Hospitals
        var h1 = new Hospital
        {
            Name = "Yangon General Hospital",
            Address = "Bogyoke Aung San Rd, Lanmadaw Township, Yangon, Myanmar",
            Phone = "+95-1-256112",
            Email = "ygh@gmail.com",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var h2 = new Hospital
        {
            Name = "Mandalay General Hospital",
            Address = "30th Street, Chan Aye Tharzan Township, Mandalay, Myanmar",
            Phone = "+95-2-35723",
            Email = "mgh@gmail.com",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var h3 = new Hospital
        {
            Name = "Bangkok General Hospital",
            Address = "2 Soi Soonvijai 7, New Petchburi Rd, Bangkok 10310",
            Phone = "026103000",
            Email = "info@bgh.co.th",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var h4 = new Hospital
        {
            Name = "Bumrungrad International",
            Address = "33 Sukhumvit 3, Wattana, Bangkok 10110",
            Phone = "026673000",
            Email = "contact@bumrungrad.com",
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Hospitals.AddRange(h1, h2, h3, h4);
        context.SaveChanges();

        // 6. Users
        var adminUser = new User
        {
            RoleId = adminRole.Id,
            UserName = "System Admin",
            Email = "admin@bdms.com",
            Password = HashPassword("admin@123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var staffUser = new User
        {
            RoleId = staffRole.Id,
            UserName = "Staff Member",
            Email = "staff@bdms.com",
            Password = HashPassword("staff@123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var donorUser = new User
        {
            RoleId = donorRole.Id,
            UserName = "Somchai Jaidee",
            Email = "somchai.j@email.com",
            Password = HashPassword("admin@123"), // donor password (admin@123)
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var hospitalStaffUser = new User
        {
            RoleId = staffRole.Id,
            HospitalId = h3.Id,
            UserName = "Niran Saengchai",
            Email = "niran.s@bgh.co.th",
            Password = HashPassword("admin@123"), // staff password (admin@123)
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, staffUser, donorUser, hospitalStaffUser);
        context.SaveChanges();

        // 7. Donors
        var donor1 = new Donor
        {
            UserId = donorUser.Id,
            NicNo = "1100600123456",
            DateOfBirth = new DateOnly(1990, 5, 14),
            Gender = "male",
            BloodGroup = "O+",
            LastDonationDate = new DateOnly(2025, 11, 20),
            EmergencyContact = "Malee Jaidee",
            EmergencyPhone = "0812345678",
            Address = "45/3 Rama IV Rd, Khlong Toei, Bangkok 10110",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var donor2 = new Donor
        {
            UserId = hospitalStaffUser.Id,
            NicNo = "1100600654321",
            DateOfBirth = new DateOnly(1985, 8, 30),
            Gender = "male",
            BloodGroup = "A+",
            LastDonationDate = null,
            EmergencyContact = "Sunisa Saengchai",
            EmergencyPhone = "0898765432",
            Address = "12 Lat Phrao Rd, Bangkok 10230",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Donors.AddRange(donor1, donor2);
        context.SaveChanges();

        // 8. Announcements
        var a1 = new Announcement
        {
            Title = "World Blood Donor Day - June 14",
            Category = "News",
            Content = "Join us on June 14 for World Blood Donor Day. Donation drives will be held at all partner hospitals across Bangkok. Walk-ins welcome.",
            IsActive = true,
            ExpiredAt = new DateOnly(2026, 6, 15),
            CreatedAt = new DateOnly(2026, 5, 22),
            UpdatedAt = new DateOnly(2026, 5, 22)
        };
        var a2 = new Announcement
        {
            Title = "Urgent: O- Blood Stock Critical",
            Category = "Emergency",
            Content = "Our O-negative inventory is critically low. All eligible donors are urged to visit the nearest hospital as soon as possible.",
            IsActive = true,
            ExpiredAt = new DateOnly(2026, 5, 20),
            CreatedAt = new DateOnly(2026, 5, 22),
            UpdatedAt = new DateOnly(2026, 5, 22)
        };
        context.Announcements.AddRange(a1, a2);
        context.SaveChanges();

        // 9. Blood Requests
        var br1 = new BloodRequest
        {
            UserId = donorUser.Id,
            HospitalId = h3.Id,
            BloodRequestCode = "BR-2026-0001",
            PatientName = "Pranee Sukjai",
            BloodGroup = "O+",
            UnitsRequired = 2,
            ContactPhone = "026103000",
            Urgency = "high",
            RequiredDate = new DateOnly(2026, 5, 10),
            Status = "approved",
            Reason = "Emergency surgery - trauma patient",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var br2 = new BloodRequest
        {
            UserId = hospitalStaffUser.Id,
            HospitalId = h4.Id,
            BloodRequestCode = "BR-2026-0002",
            PatientName = "David Lim",
            BloodGroup = "B+",
            UnitsRequired = 1,
            ContactPhone = "026673000",
            Urgency = "medium",
            RequiredDate = new DateOnly(2026, 5, 15),
            Status = "pending",
            Reason = "Scheduled cardiac procedure",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.BloodRequests.AddRange(br1, br2);
        context.SaveChanges();

        // 10. Donations
        var d1 = new Donation
        {
            DonorId = donor1.Id,
            HospitalId = h3.Id,
            BloodRequestId = br1.Id,
            CreatedBy = hospitalStaffUser.Id,
            DonationCode = "DON-2026-0001",
            BloodGroup = "O+",
            UnitsDonated = 1,
            DonationDate = new DateOnly(2026, 5, 2),
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var d2 = new Donation
        {
            DonorId = donor2.Id,
            HospitalId = h4.Id,
            BloodRequestId = null,
            CreatedBy = hospitalStaffUser.Id,
            DonationCode = "DON-2026-0002",
            BloodGroup = "A+",
            UnitsDonated = 1,
            DonationDate = new DateOnly(2026, 5, 2),
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Donations.AddRange(d1, d2);
        context.SaveChanges();

        // 11. Appointments
        var appt1 = new Appointment
        {
            UserId = donorUser.Id,
            HospitalId = h3.Id,
            DonationId = d1.Id,
            BloodRequestId = br1.Id,
            AppointmentDate = new DateOnly(2026, 5, 2),
            AppointmentTime = new TimeOnly(9, 0, 0),
            Status = "completed",
            Remarks = "Donor arrived on time. No complications.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var appt2 = new Appointment
        {
            UserId = hospitalStaffUser.Id,
            HospitalId = h4.Id,
            DonationId = d2.Id,
            BloodRequestId = null,
            AppointmentDate = new DateOnly(2026, 5, 7),
            AppointmentTime = new TimeOnly(14, 0, 0),
            Status = "scheduled",
            Remarks = "First-time donation screening appointment.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Appointments.AddRange(appt1, appt2);
        context.SaveChanges();

        // 12. Medical Records
        var mr1 = new MedicalRecord
        {
            DonationId = d1.Id,
            HospitalId = h3.Id,
            HemoglobinLevel = 14.20m,
            HivResult = "negative",
            HepatitisBResult = "negative",
            HepatitisCResult = "negative",
            MalariaResult = "negative",
            SyphilisResult = "negative",
            ScreeningStatus = "passed",
            ScreeningNotes = "All screening tests passed. Donor fit for donation.",
            ScreenedBy = hospitalStaffUser.Id,
            ScreeningAt = new DateTime(2026, 5, 2, 8, 45, 0),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mr2 = new MedicalRecord
        {
            DonationId = d2.Id,
            HospitalId = h4.Id,
            HemoglobinLevel = 13.80m,
            HivResult = "negative",
            HepatitisBResult = "negative",
            HepatitisCResult = "negative",
            MalariaResult = "negative",
            SyphilisResult = "negative",
            ScreeningStatus = "pending",
            ScreeningNotes = "Awaiting lab confirmation.",
            ScreenedBy = null,
            ScreeningAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.MedicalRecords.AddRange(mr1, mr2);
        context.SaveChanges();

        // 13. Blood Inventories
        var inv1 = new BloodInventory
        {
            DonationId = d1.Id,
            HospitalId = h3.Id,
            BloodGroup = "O+",
            Units = 1,
            CollectedAt = new DateOnly(2026, 5, 2),
            ExpiredAt = new DateOnly(2026, 6, 2),
            Status = "used",
            RequestId = br1.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var inv2 = new BloodInventory
        {
            DonationId = d2.Id,
            HospitalId = h4.Id,
            BloodGroup = "A+",
            Units = 1,
            CollectedAt = new DateOnly(2026, 5, 2),
            ExpiredAt = new DateOnly(2026, 6, 2),
            Status = "available",
            RequestId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.BloodInventories.AddRange(inv1, inv2);
        context.SaveChanges();

        // 14. Certificates
        var cert1 = new Certificate
        {
            UserId = donorUser.Id,
            CertificateTitle = "Blood Donation Certificate - May 2026",
            CertificateDescription = "Awarded to Somchai Jaidee for completing a successful blood donation on 2 May 2026.",
            CertificateData = "{\"donor\":\"Somchai Jaidee\",\"date\":\"2026-05-02\",\"hospital\":\"Bangkok General Hospital\",\"bloodGroup\":\"O+\",\"units\":1}",
            CreatedAt = new DateOnly(2026, 5, 22),
            UpdatedAt = new DateOnly(2026, 5, 22)
        };
        var cert2 = new Certificate
        {
            UserId = hospitalStaffUser.Id,
            CertificateTitle = "Staff Appreciation Certificate - Q1 2026",
            CertificateDescription = "Awarded to Niran Saengchai for outstanding contribution to the blood donation program.",
            CertificateData = "{\"recipient\":\"Niran Saengchai\",\"period\":\"Q1 2026\",\"hospital\":\"Bangkok General Hospital\"}",
            CreatedAt = new DateOnly(2026, 5, 22),
            UpdatedAt = new DateOnly(2026, 5, 22)
        };
        context.Certificates.AddRange(cert1, cert2);
        context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] key = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: 100000,
            hashAlgorithm: System.Security.Cryptography.HashAlgorithmName.SHA256,
            outputLength: 32
        );

        var hashBytes = new byte[16 + 32];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);
        Buffer.BlockCopy(key, 0, hashBytes, 16, 32);
        return Convert.ToBase64String(hashBytes);
    }
}
