using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.MedicalRecord;

public interface IMedicalRecordService
{
    Task<Result<List<MedicalRecordRespModel>>> GetAll(CancellationToken ct);
    Task<Result<MedicalRecordRespModel>> GetById(int id, CancellationToken ct);
    Task<Result<MedicalRecordRespModel>> Create(CreateMedicalRecordCommand command, CancellationToken ct);
    Task<Result<MedicalRecordRespModel>> Update(UpdateMedicalRecordCommand command, CancellationToken ct);
    Task<Result<string>> Delete(int id, CancellationToken ct);
}
