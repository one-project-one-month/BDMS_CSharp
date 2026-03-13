using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Commands;

public class UpdateMedicalRecordCommand : IRequest<Result<MedicalRecordRespModel>>
{
    public int Id { get; set; }
    public int DonationId { get; set; }
    public int HospitalId { get; set; }
    public decimal? HemoglobinLevel { get; set; }
    public string? HivResult { get; set; }
    public string? HepatitisBResult { get; set; }
    public string? HepatitisCResult { get; set; }
    public string? MalariaResult { get; set; }
    public string? SyphilisResult { get; set; }
    public string? ScreeningStatus { get; set; }
    public string? ScreeningNotes { get; set; }
    public int? ScreenedBy { get; set; }
    public DateTime? ScreeningAt { get; set; }
}
