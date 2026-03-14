using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Domain.Features.MedicalRecord.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Handlers;

public class GetAllMedicalRecordsHandler : IRequestHandler<GetAllMedicalRecordsQuery, Result<List<MedicalRecordRespModel>>>
{
    private readonly IMedicalRecordService _service;

    public GetAllMedicalRecordsHandler(IMedicalRecordService service)
    {
        _service = service;
    }

    public async Task<Result<List<MedicalRecordRespModel>>> Handle(GetAllMedicalRecordsQuery request, CancellationToken ct)
        => await _service.GetAll(ct);
}
