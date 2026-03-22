using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Domain.Features.MedicalRecord.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Handlers;

public class GetMedicalRecordByIdHandler : IRequestHandler<GetMedicalRecordByIdQuery, Result<MedicalRecordRespModel>>
{
    private readonly IMedicalRecordService _service;

    public GetMedicalRecordByIdHandler(IMedicalRecordService service)
    {
        _service = service;
    }

    public async Task<Result<MedicalRecordRespModel>> Handle(GetMedicalRecordByIdQuery request, CancellationToken ct)
        => await _service.GetById(request.Id, ct);
}
