using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Commands;

public class UpdateAppointmentStatusCommand : IRequest<Result<AppointmentRespModel>>
{
    public int Id { get; set; }
    public EnumAppointmentStatus Status { get; set; }
}