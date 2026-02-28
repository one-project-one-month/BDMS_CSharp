namespace BDMS.Application.Donors;

public class DonorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public DateTime? LastDonationDate { get; set; }
    public bool CanDonate { get; set; }
}
