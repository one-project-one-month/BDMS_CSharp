namespace BDMS.Domain.Features.Appointment.Models;

public class AppointmentReqModel
{
    public string? Remarks { get; set; }
}

public class UpdateAppointmentStatusReqModel
{
    public string Status { get; set; } = string.Empty;
}