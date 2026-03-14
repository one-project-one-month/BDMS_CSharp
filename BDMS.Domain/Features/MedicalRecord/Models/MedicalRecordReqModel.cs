using BDMS.Shared.Enums;

namespace BDMS.Domain.Features.MedicalRecord.Models;

public class MedicalRecordReqModel
{
    public int Id { get; set; }
    public int DonationId { get; set; }
    public int HospitalId { get; set; }
    public decimal? HemoglobinLevel { get; set; }
    public EnumMedicalRecordResult HivResult { get; set; }
    public EnumMedicalRecordResult HepatitisBResult { get; set; }
    public EnumMedicalRecordResult HepatitisCResult { get; set; }
    public EnumMedicalRecordResult MalariaResult { get; set; }
    public EnumMedicalRecordResult SyphilisResult { get; set; }
    public EnumMedicalRecordScreeningStatus ScreeningStatus { get; set; }
    public string? ScreeningNotes { get; set; }
    public int? ScreenedBy { get; set; }
    public DateTime? ScreeningAt { get; set; }
}
