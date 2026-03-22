using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donations.Handlers;

public class UpdateDonationStatusHandler : IRequestHandler<Commands.UpdateDonationStatusCommand, Result<DonationRespModel>>
{
    private readonly IDonationService _donationService;

    public UpdateDonationStatusHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.UpdateDonationStatusCommand reqModel, CancellationToken cancellation)
    {

        return await _donationService.UpdateDonationStatus(reqModel);

    }
}

