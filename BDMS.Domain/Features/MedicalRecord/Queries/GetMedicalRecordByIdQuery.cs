using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Queries;

public class GetMedicalRecordByIdQuery : IRequest<Result<MedicalRecordRespModel>>
{
    public int Id { get; set; }
}
