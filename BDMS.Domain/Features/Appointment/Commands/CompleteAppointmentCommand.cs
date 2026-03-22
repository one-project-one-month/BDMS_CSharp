using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Commands;

public class CompleteAppointmentCommand : IRequest<Result<AppointmentRespModel>>
{
    public int Id { get; set; }
}