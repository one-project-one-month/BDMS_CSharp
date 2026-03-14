using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Domain.Features.Appointment.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Handlers;

public class GetAllAppointmentHandler : IRequestHandler<GetAllAppointmentQuery, Result<List<AppointmentRespModel>>>
{
    private readonly IAppointmentService _appointmentService;

    public GetAllAppointmentHandler(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<Result<List<AppointmentRespModel>>> Handle(GetAllAppointmentQuery request, CancellationToken ct)
    {
        return await _appointmentService.GetAllAppointments(ct);
    }
}