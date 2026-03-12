using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class CreateBloodRequestHandler : IRequestHandler<CreateBloodRequestCommand, Result<BloodRequestRespModel>>
{
    private readonly BloodRequestService _service;

    public CreateBloodRequestHandler(BloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(CreateBloodRequestCommand request, CancellationToken ct)
        => await _service.Create(request, ct);
}
