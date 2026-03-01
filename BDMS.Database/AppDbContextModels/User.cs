using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class User
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int? HospitalId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<BloodRequest> BloodRequestApprovedByNavigations { get; set; } = new List<BloodRequest>();

    public virtual ICollection<BloodRequest> BloodRequestUsers { get; set; } = new List<BloodRequest>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Donation> DonationApprovedByNavigations { get; set; } = new List<Donation>();

    public virtual ICollection<Donation> DonationCreatedByNavigations { get; set; } = new List<Donation>();

    public virtual Donor? Donor { get; set; }

    public virtual Hospital? Hospital { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual Role Role { get; set; } = null!;
}
