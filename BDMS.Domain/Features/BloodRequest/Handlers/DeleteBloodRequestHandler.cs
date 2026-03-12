using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Shared;
using MediatR;
using BDMS.Domain.Features.BloodRequest;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class DeleteBloodRequestHandler : IRequestHandler<DeleteBloodRequestCommand, Result<string>>
{
    private readonly IBloodRequestService _service;

    public DeleteBloodRequestHandler(IBloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<string>> Handle(DeleteBloodRequestCommand request, CancellationToken ct)
        => await _service.Delete(request.Id, ct);
}
