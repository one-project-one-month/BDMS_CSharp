using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Queries;

public class GetAppointmentByIdQuery : IRequest<Result<AppointmentRespModel>>
{
    public int Id { get; set; }
}