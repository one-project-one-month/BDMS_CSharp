namespace BDMS.Shared.Enums;

public static class BloodRequestEnumExtensions
{
    public static string ToDatabaseValue(this EnumBloodGroup bloodGroup)
    {
        return bloodGroup switch
        {
            EnumBloodGroup.APositive => "A+",
            EnumBloodGroup.ANegative => "A-",
            EnumBloodGroup.BPositive => "B+",
            EnumBloodGroup.BNegative => "B-",
            EnumBloodGroup.ABPositive => "AB+",
            EnumBloodGroup.ABNegative => "AB-",
            EnumBloodGroup.OPositive => "O+",
            EnumBloodGroup.ONegative => "O-",
            _ => string.Empty
        };
    }

    public static EnumBloodGroup ToBloodGroupEnum(this string? bloodGroup)
    {
        if (string.IsNullOrWhiteSpace(bloodGroup))
            return EnumBloodGroup.None;

        return bloodGroup.Trim().ToUpperInvariant() switch
        {
            "A+" => EnumBloodGroup.APositive,
            "A-" => EnumBloodGroup.ANegative,
            "B+" => EnumBloodGroup.BPositive,
            "B-" => EnumBloodGroup.BNegative,
            "AB+" => EnumBloodGroup.ABPositive,
            "AB-" => EnumBloodGroup.ABNegative,
            "O+" => EnumBloodGroup.OPositive,
            "O-" => EnumBloodGroup.ONegative,
            _ => EnumBloodGroup.None
        };
    }

    public static string ToDatabaseValue(this EnumBloodRequestStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }
}
