using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Handlers;

public class CreateMedicalRecordHandler : IRequestHandler<CreateMedicalRecordCommand, Result<MedicalRecordRespModel>>
{
    private readonly IMedicalRecordService _service;

    public CreateMedicalRecordHandler(IMedicalRecordService service)
    {
        _service = service;
    }

    public async Task<Result<MedicalRecordRespModel>> Handle(CreateMedicalRecordCommand request, CancellationToken ct)
        => await _service.Create(request, ct);
}
