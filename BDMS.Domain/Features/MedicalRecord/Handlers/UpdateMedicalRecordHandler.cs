using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Handlers;

public class UpdateMedicalRecordHandler : IRequestHandler<UpdateMedicalRecordCommand, Result<MedicalRecordRespModel>>
{
    private readonly IMedicalRecordService _service;

    public UpdateMedicalRecordHandler(IMedicalRecordService service)
    {
        _service = service;
    }

    public async Task<Result<MedicalRecordRespModel>> Handle(UpdateMedicalRecordCommand request, CancellationToken ct)
        => await _service.Update(request, ct);
}
