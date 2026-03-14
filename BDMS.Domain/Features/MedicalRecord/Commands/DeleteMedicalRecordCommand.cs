using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Commands;

public class DeleteMedicalRecordCommand : IRequest<Result<string>>
{
    public int Id { get; set; }
}
