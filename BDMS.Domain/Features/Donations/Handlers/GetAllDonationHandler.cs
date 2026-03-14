using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donation.Handlers;

public class GetAllDonationHandler : IRequestHandler<Queries.GetAllDonationQuery, Result<List<DonationRespModel>>>
{
    private readonly IDonationService _donationService;

    public GetAllDonationHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<List<DonationRespModel>>> Handle(Queries.GetAllDonationQuery query, CancellationToken cancellationToken)
    {
        return await _donationService.GetAllDonations();
    }
}
