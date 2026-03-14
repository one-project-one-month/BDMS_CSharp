using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class DeleteAppointmentHandler : IRequestHandler<DeleteAppointmentCommand, Result<string>>
{
    private readonly IAppointmentService _appointmentService;

    public DeleteAppointmentHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<string>> Handle(DeleteAppointmentCommand request, CancellationToken ct)
    {
        return await _appointmentService.DeleteAppointment(request, ct);
    }
}
