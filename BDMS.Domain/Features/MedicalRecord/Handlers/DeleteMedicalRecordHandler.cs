using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Handlers;

public class DeleteMedicalRecordHandler : IRequestHandler<DeleteMedicalRecordCommand, Result<string>>
{
    private readonly IMedicalRecordService _service;

    public DeleteMedicalRecordHandler(IMedicalRecordService service)
    {
        _service = service;
    }

    public async Task<Result<string>> Handle(DeleteMedicalRecordCommand request, CancellationToken ct)
        => await _service.Delete(request.Id, ct);
}
