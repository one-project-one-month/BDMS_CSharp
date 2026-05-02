namespace BDMS.Domain.Features.Appointment.Models;

public class AppointmentReqModel
{
    public string? Remarks { get; set; }
}

public class UpdateAppointmentStatusReqModel
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateAppointmentTimeReqModel
{
    public string AppointmentDate { get; set; } = string.Empty; // yyyy-MM-dd
    public string AppointmentTime { get; set; } = string.Empty; // HH:mm
}
