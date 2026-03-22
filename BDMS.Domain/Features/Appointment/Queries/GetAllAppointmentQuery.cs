using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Queries;

public class GetAllAppointmentQuery : IRequest<Result<List<AppointmentRespModel>>>
{
    public int? HospitalId { get; set; }
    public DateOnly? AppointmentDate { get; set; }
}