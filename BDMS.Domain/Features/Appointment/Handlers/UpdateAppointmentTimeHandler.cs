using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class UpdateAppointmentTimeHandler : IRequestHandler<UpdateAppointmentTimeCommand, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public UpdateAppointmentTimeHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(UpdateAppointmentTimeCommand request, CancellationToken ct)
    {
        return await _appointmentService.UpdateAppointmentTime(request, ct);
    }
}
