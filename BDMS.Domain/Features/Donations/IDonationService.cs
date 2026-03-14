using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Donation
{
    public interface IDonationService
    {
        Task<Result<DonationRespModel>> CreateDonation(DonationCreateReqModel reqModel);
        Task<Result<DonationRespModel>> DeleteDonation(int donationId);
        Task<Result<List<DonationRespModel>>> GetAllDonations();
        Task<Result<DonationRespModel>> GetDonationById(int donationId);
        Task<Result<DonationRespModel>> UpdateDonation(DonationUpdateReqModel reqModel);
    }
}