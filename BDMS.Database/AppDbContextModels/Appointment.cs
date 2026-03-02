using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int HospitalId { get; set; }

    public int? DonationId { get; set; }

    public int? BloodRequestId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual BloodRequest? BloodRequest { get; set; }

    public virtual Donation? Donation { get; set; }

    public virtual Hospital Hospital { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
