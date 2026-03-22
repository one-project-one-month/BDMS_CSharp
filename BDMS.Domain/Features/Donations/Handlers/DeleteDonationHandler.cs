using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donations.Handlers;

public class DeleteDonationHandler : IRequestHandler<Commands.DeleteDonationCommand, Result<DonationRespModel>>
{
    private readonly IDonationService _donationService;

    public DeleteDonationHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.DeleteDonationCommand reqModel, CancellationToken cancellationToken)
    {
        return await _donationService.DeleteDonation(reqModel.Id);
    }
}
