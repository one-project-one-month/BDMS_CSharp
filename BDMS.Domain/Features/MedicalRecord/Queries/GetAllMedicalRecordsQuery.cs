using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.MedicalRecord.Queries;

public class GetAllMedicalRecordsQuery : IRequest<Result<List<MedicalRecordRespModel>>>;
