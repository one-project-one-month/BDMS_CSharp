using BDMS.Domain.Features.Appointment.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Appointment;

public interface IAppointmentService
{
    Task<Result<List<AppointmentRespModel>>> GetAllAppointments(CancellationToken ct);
    
    Task<Result<AppointmentRespModel>> GetAppointmentById(Queries.GetAppointmentByIdQuery request, CancellationToken ct);
    
    Task<Result<AppointmentRespModel>> CreateDonationAppointment(Commands.CreateDonationAppointmentCommand request,
        CancellationToken ct);
    
    Task<Result<AppointmentRespModel>> CreateBloodRequestAppointment(Commands.CreateBloodRequestAppointmentCommand request,
        CancellationToken ct);

    Task<Result<AppointmentRespModel>> UpdateAppointmentStatus(Commands.UpdateAppointmentStatusCommand request, CancellationToken ct);
    
    Task<Result<AppointmentRespModel>> CompleteAppointment(Commands.CompleteAppointmentCommand request, CancellationToken ct);

    Task<Result<string>> DeleteAppointment(Commands.DeleteAppointmentCommand request, CancellationToken ct);
}
