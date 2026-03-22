using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Commands;

public class UpdateBloodRequestStatusCommand : IRequest<Result<BloodRequestRespModel>>
{
    public int Id { get; set; }
    public EnumBloodRequestStatus Status { get; set; }
    public int? DonorId { get; set; }
}
