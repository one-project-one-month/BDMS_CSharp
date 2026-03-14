using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class CreateDonationAppointmentHandler : IRequestHandler<CreateDonationAppointmentCommand, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public CreateDonationAppointmentHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(CreateDonationAppointmentCommand request, CancellationToken ct)
    {
        return await _appointmentService.CreateDonationAppointment(request, ct);
    }
}
