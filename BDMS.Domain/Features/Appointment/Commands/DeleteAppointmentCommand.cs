using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Commands;

public class DeleteAppointmentCommand : IRequest<Result<string>>
{
    public int Id { get; set; }
}
