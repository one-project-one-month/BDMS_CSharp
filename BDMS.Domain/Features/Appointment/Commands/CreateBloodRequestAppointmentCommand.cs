using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Commands;

public class CreateBloodRequestAppointmentCommand : IRequest<Result<AppointmentRespModel>>
{
    public int BloodRequestId { get; set; }
    public string? Remarks { get; set; }
}
