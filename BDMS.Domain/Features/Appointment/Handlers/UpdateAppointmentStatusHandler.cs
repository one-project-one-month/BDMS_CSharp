using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public UpdateAppointmentStatusHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(UpdateAppointmentStatusCommand request,
        CancellationToken ct)
    {
        return await _appointmentService.UpdateAppointmentStatus(request, ct);
    }
}