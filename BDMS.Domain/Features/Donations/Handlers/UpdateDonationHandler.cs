using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donations.Handlers;

public class UpdateDonationHandler : IRequestHandler<Commands.UpdateDonationCommand, Result<DonationRespModel>>
{
    private readonly IDonationService _donationService;

    public UpdateDonationHandler(IDonationService donationService)
    {
        _donationService = donationService;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.UpdateDonationCommand reqModel, CancellationToken cancellationToken)
    {
        var donationRequest = new DonationUpdateReqModel()
        {
            Id = reqModel.Id,
            DonorId = reqModel.DonorId,
            HospitalId = reqModel.HospitalId,
            BloodRequestId = reqModel.BloodRequestId,
            DonationCode = reqModel.DonationCode,
            BloodGroup = reqModel.BloodGroup,
            UnitsDonated = reqModel.UnitsDonated,
            DonationDate = reqModel.DonationDate,
            Status = reqModel.Status,
            ApprovedBy = reqModel.ApprovedBy,
            ApprovedAt = reqModel.ApprovedAt,
            Remarks = reqModel.Remarks
        };
        return await _donationService.UpdateDonation(donationRequest);
    }
}
