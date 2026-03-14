using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;
using MediatR;
using BDMS.Domain.Features.BloodRequest;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class CreateBloodRequestHandler : IRequestHandler<CreateBloodRequestCommand, Result<BloodRequestRespModel>>
{
    private readonly IBloodRequestService _service;

    public CreateBloodRequestHandler(IBloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<BloodRequestRespModel>> Handle(CreateBloodRequestCommand request, CancellationToken ct)
        => await _service.Create(request, ct);
}
