using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Donor
{
    public interface IDonorService
    {
        Task<Result<DonorRespModel>> CreateDonor(DonorReqModel reqModel);
        Task<Result<DonorRespModel>> DeleteDonor(int donorId);
        Task<Result<List<DonorRespModel>>> GetAllDonors();
        Task<Result<DonorRespModel>> GetDonorById(int donorId);
        Task<Result<DonorRespModel>> UpdateDonor(DonorReqModel reqModel);
    }
}