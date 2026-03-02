using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class BloodInventory
{
    public int Id { get; set; }

    public int DonationId { get; set; }

    public int HospitalId { get; set; }

    public string BloodGroup { get; set; } = null!;

    public int Units { get; set; }

    public DateOnly? CollectedAt { get; set; }

    public DateOnly? ExpiredAt { get; set; }

    public string Status { get; set; } = null!;

    public int? RequestId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Donation Donation { get; set; } = null!;

    public virtual Hospital Hospital { get; set; } = null!;

    public virtual BloodRequest? Request { get; set; }
}
