using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class CompleteAppointmentHandler : IRequestHandler<CompleteAppointmentCommand, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public CompleteAppointmentHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(CompleteAppointmentCommand request,
        CancellationToken ct)
    {
        return await _appointmentService.CompleteAppointment(request, ct);
    }
}