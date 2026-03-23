using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donations.Handlers;

public class GetDonationByDateAndHospi : IRequestHandler<Queries.GetDonationByDateAndHospitalQuery, Result<List<DonationRespModel>>>
{
    private readonly IDonationService _dontionService;

    public GetDonationByDateAndHospi(IDonationService dontionService)
    {
        _dontionService = dontionService;
    }

    public async Task<Result<List<DonationRespModel>>> Handle(Queries.GetDonationByDateAndHospitalQuery reqModel, CancellationToken cancellationToken)
    {
        return await _dontionService.GetDonationByDateAndHospi(reqModel);
    }
}
