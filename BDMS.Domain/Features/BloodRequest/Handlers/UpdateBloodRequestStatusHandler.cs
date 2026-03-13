using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;
using BDMS.Domain.Features.BloodRequest;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class UpdateBloodRequestStatusHandler : IRequestHandler<UpdateBloodRequestStatusCommand, Result<BloodRequestRespModel>>
{
    private readonly IBloodRequestService _service;

    public UpdateBloodRequestStatusHandler(IBloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(UpdateBloodRequestStatusCommand request, CancellationToken ct)
        => await _service.UpdateStatus(request, ct);
}
