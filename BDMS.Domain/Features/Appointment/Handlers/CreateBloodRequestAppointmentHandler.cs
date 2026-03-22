using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class CreateBloodRequestAppointmentHandler : IRequestHandler<CreateBloodRequestAppointmentCommand, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public CreateBloodRequestAppointmentHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(CreateBloodRequestAppointmentCommand request,CancellationToken ct)
    {
        return await _appointmentService.CreateBloodRequestAppointment(request, ct);
    }
}