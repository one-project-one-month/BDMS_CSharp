using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.BloodRequest
{
    public interface IBloodRequestService
    {
        Task<Result<BloodRequestRespModel>> Create(CreateBloodRequestCommand command, CancellationToken ct);
        Task<Result<string>> Delete(int id, CancellationToken ct);
        Task<Result<List<BloodRequestRespModel>>> GetAll(CancellationToken ct);
        Task<Result<BloodRequestRespModel>> GetById(int id, CancellationToken ct);
        Task<Result<BloodRequestRespModel>> Update(UpdateBloodRequestCommand command, CancellationToken ct);
        Task<Result<BloodRequestRespModel>> UpdateStatus(UpdateBloodRequestStatusCommand command, CancellationToken ct);
    }
}