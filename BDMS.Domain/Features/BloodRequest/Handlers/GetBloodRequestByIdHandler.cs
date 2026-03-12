using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.BloodRequest.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class GetBloodRequestByIdHandler : IRequestHandler<GetBloodRequestByIdQuery, Result<BloodRequestRespModel>>
{
    private readonly BloodRequestService _service;

    public GetBloodRequestByIdHandler(BloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(GetBloodRequestByIdQuery request, CancellationToken ct)
        => await _service.GetById(request.Id, ct);
}
