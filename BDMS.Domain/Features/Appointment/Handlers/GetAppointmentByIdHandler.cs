using BDMS.Domain.Features.Appointment.Models;
using BDMS.Domain.Features.Appointment.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentRespModel>>
{
    private readonly IAppointmentService _appointmentService;

    public GetAppointmentByIdHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<AppointmentRespModel>> Handle(GetAppointmentByIdQuery request,
        CancellationToken ct)
    {
        return await _appointmentService.GetAppointmentById(request, ct);
    }
}