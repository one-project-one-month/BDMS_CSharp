namespace BDMS.Domain.Entities;

public class BloodDonor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public DateTime? LastDonationDate { get; set; }

    public bool CanDonate()
    {
        if (!LastDonationDate.HasValue)
        {
            return true;
        }

        return LastDonationDate.Value <= DateTime.UtcNow.AddMonths(-3);
    }
}
