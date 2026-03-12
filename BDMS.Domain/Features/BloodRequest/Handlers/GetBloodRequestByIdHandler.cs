using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.BloodRequest.Queries;
using BDMS.Shared;
using MediatR;
using BDMS.Domain.Features.BloodRequest;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class GetBloodRequestByIdHandler : IRequestHandler<GetBloodRequestByIdQuery, Result<BloodRequestRespModel>>
{
    private readonly IBloodRequestService _service;

    public GetBloodRequestByIdHandler(IBloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(GetBloodRequestByIdQuery request, CancellationToken ct)
        => await _service.GetById(request.Id, ct);
}
