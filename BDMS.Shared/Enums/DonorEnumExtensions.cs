namespace BDMS.Shared.Enums;

public static class DonorEnumExtensions
{
    public static string ToDatabaseValue(this EnumDonorStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    public static EnumDonorStatus ToDonorStatusEnum(this string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return EnumDonorStatus.None;

        return status.Trim().ToLowerInvariant() switch
        {
            "pending" => EnumDonorStatus.Pending,
            "approved" => EnumDonorStatus.Approved,
            _ => EnumDonorStatus.None
        };
    }
}
