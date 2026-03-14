using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Queries;

public class GetAllBloodRequestsQuery : IRequest<Result<List<BloodRequestRespModel>>>;
