using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class UpdateBloodRequestHandler : IRequestHandler<UpdateBloodRequestCommand, Result<BloodRequestRespModel>>
{
    private readonly BloodRequestService _service;

    public UpdateBloodRequestHandler(BloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(UpdateBloodRequestCommand request, CancellationToken ct)
        => await _service.Update(request, ct);
}
