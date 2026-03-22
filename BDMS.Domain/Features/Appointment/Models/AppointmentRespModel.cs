using BDMS.Shared.Enums;

namespace BDMS.Domain.Features.Appointment.Models;

public class AppointmentRespModel
{
    public int UserId { get; set; }

    public int HospitalId { get; set; }

    public int? DonationId { get; set; }

    public int? BloodRequestId { get; set; }
    
    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public EnumAppointmentStatus Status { get; set; }

    public string? Remarks { get; set; }
}