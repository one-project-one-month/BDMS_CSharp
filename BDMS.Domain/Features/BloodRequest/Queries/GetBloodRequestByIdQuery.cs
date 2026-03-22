using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Queries;

public class GetBloodRequestByIdQuery : IRequest<Result<BloodRequestRespModel>>
{
    public int Id { get; set; }
}
