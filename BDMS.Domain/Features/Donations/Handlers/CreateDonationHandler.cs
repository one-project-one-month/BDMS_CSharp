using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donations.Handlers;

public class CreateDonationHandler : IRequestHandler<Commands.CreateDonationCommand, Result<DonationRespModel>>
{
    private readonly IDonationService _donationService;

    public CreateDonationHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.CreateDonationCommand reqModel, CancellationToken cancellationToken)
    {
        var donationRequest = new DonationCreateReqModel()
        {
            DonorId = reqModel.DonorId,
            HospitalId = reqModel.HospitalId,
            BloodRequestId = reqModel.BloodRequestId,
            CreatedBy = reqModel.CreatedBy,
            DonationCode = reqModel.DonationCode,
            BloodGroup = reqModel.BloodGroup,
            UnitsDonated = reqModel.UnitsDonated,
            DonationDate = reqModel.DonationDate,
            Remarks = reqModel.Remarks,
        };

        return await _donationService.CreateDonation(donationRequest);
    }
}
