using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donations.Handlers;

public class GetDonationByIdHandler : IRequestHandler<Queries.GetDonationByIdQuery, Result<DonationRespModel>>
{
    private readonly IDonationService _donationService;

    public GetDonationByIdHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<DonationRespModel>> Handle(Queries.GetDonationByIdQuery query, CancellationToken cancellationToken)
    {
        return await _donationService.GetDonationById(query.Id);
    }
}
