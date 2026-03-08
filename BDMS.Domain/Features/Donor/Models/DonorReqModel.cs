namespace BDMS.Domain.Features.Donor.Models;

public class DonorReqModel
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
}
