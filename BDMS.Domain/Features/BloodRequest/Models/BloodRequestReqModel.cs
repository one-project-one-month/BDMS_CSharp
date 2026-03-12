namespace BDMS.Domain.Features.BloodRequest.Models;

public class BloodRequestReqModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int HospitalId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public int UnitsRequired { get; set; }
    public string? ContactPhone { get; set; }
    public string Urgency { get; set; } = "low";
    public DateOnly? RequiredDate { get; set; }
    public string? Reason { get; set; }
}

public class UpdateBloodRequestStatusReqModel
{
    public string Status { get; set; } = string.Empty;
    public int? DonorId { get; set; }
}
