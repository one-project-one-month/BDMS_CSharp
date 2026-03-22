using M = BDMS.Database.AppDbContextModels;

namespace BDMS.Domain.Features.Donor.Models;

public class DonorRespModel
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string NicNo { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string BloodGroup { get; set; } = null!;

    public DateOnly? LastDonationDate { get; set; }

    public string? Remarks { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<M.Donation> Donations { get; set; } = new List<M.Donation>();

    public virtual M.User User { get; set; } = null!;
}
