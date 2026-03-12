using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.BloodRequest.Handlers;

public class DeleteBloodRequestHandler : IRequestHandler<DeleteBloodRequestCommand, Result<string>>
{
    private readonly BloodRequestService _service;

    public DeleteBloodRequestHandler(BloodRequestService service)
    {
        _service = service;
    }

    public async Task<Result<string>> Handle(DeleteBloodRequestCommand request, CancellationToken ct)
        => await _service.Delete(request.Id, ct);
}
