using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.BloodRequest.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class GetAllBloodRequestsHandler : IRequestHandler<GetAllBloodRequestsQuery, Result<List<BloodRequestRespModel>>>
{
    private readonly BloodRequestService _service;

    public GetAllBloodRequestsHandler(BloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<List<BloodRequestRespModel>>> Handle(GetAllBloodRequestsQuery request, CancellationToken ct)
        => await _service.GetAll(ct);
}
