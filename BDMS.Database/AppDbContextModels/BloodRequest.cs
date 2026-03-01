using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class BloodRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int HospitalId { get; set; }

    public string PatientName { get; set; } = null!;

    public string BloodGroup { get; set; } = null!;

    public int UnitsRequired { get; set; }

    public string? ContactPhone { get; set; }

    public string Urgency { get; set; } = null!;

    public DateOnly? RequiredDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual ICollection<BloodInventory> BloodInventories { get; set; } = new List<BloodInventory>();

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual Hospital Hospital { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
