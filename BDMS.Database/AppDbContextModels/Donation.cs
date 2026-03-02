using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class Donation
{
    public int Id { get; set; }

    public int DonorId { get; set; }

    public int HospitalId { get; set; }

    public int? BloodRequestId { get; set; }

    public int CreatedBy { get; set; }

    public string BloodGroup { get; set; } = null!;

    public int? UnitsDonated { get; set; }

    public DateOnly? DonationDate { get; set; }

    public string Status { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual BloodInventory? BloodInventory { get; set; }

    public virtual BloodRequest? BloodRequest { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Donor Donor { get; set; } = null!;

    public virtual Hospital Hospital { get; set; } = null!;

    public virtual MedicalRecord? MedicalRecord { get; set; }
}
