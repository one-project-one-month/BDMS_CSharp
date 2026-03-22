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

    public static string ToDatabaseValue(this EnumBloodRequestUrgency urgency)
    {
        return urgency.ToString().ToLowerInvariant();
    }

    public static EnumBloodRequestUrgency ToUrgencyEnum(this string? urgency)
    {
        if (string.IsNullOrWhiteSpace(urgency))
            return EnumBloodRequestUrgency.None;

        return urgency.Trim().ToLowerInvariant() switch
        {
            "low" => EnumBloodRequestUrgency.Low,
            "medium" => EnumBloodRequestUrgency.Medium,
            "high" => EnumBloodRequestUrgency.High,
            "critical" => EnumBloodRequestUrgency.Critical,
            _ => EnumBloodRequestUrgency.None
        };
    }
}
