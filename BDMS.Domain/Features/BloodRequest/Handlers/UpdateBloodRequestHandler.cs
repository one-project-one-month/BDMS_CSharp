using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;
using BDMS.Domain.Features.BloodRequest;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class UpdateBloodRequestHandler : IRequestHandler<UpdateBloodRequestCommand, Result<BloodRequestRespModel>>
{
    private readonly IBloodRequestService _service;

    public UpdateBloodRequestHandler(IBloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(UpdateBloodRequestCommand request, CancellationToken ct)
        => await _service.Update(request, ct);
}
