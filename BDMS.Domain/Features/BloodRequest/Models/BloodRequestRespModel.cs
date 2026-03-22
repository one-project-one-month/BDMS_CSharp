using BDMS.Shared.Enums;

namespace BDMS.Domain.Features.BloodRequest.Models;

public class BloodRequestRespModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int HospitalId { get; set; }
    public string? BloodRequestCode { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public EnumBloodGroup BloodGroup { get; set; }
    public int UnitsRequired { get; set; }
    public string? ContactPhone { get; set; }
    public string Urgency { get; set; } = string.Empty;
    public DateOnly? RequiredDate { get; set; }
    public EnumBloodRequestStatus Status { get; set; }
    public string? Reason { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
