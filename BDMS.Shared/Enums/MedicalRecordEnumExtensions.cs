namespace BDMS.Shared.Enums;

public static class MedicalRecordEnumExtensions
{
    public static string? ToDatabaseValue(this EnumMedicalRecordResult result)
    {
        return result == EnumMedicalRecordResult.None
            ? null
            : result.ToString().ToLowerInvariant();
    }

    public static string? ToDatabaseValue(this EnumMedicalRecordScreeningStatus status)
    {
        return status == EnumMedicalRecordScreeningStatus.None
            ? null
            : status.ToString().ToLowerInvariant();
    }
}
