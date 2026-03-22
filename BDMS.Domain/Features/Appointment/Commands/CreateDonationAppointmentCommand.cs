using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Appointment.Commands;

public class CreateDonationAppointmentCommand : IRequest<Result<AppointmentRespModel>>
{
    public int DonationId { get; set; }
    public string? Remarks { get; set; }
}
