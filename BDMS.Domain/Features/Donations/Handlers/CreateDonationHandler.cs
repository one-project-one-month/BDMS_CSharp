using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BDMS.Domain.Features.Donations.Handlers;

public class CreateDonationHandler : IRequestHandler<Commands.CreateDonationCommand, Result<DonationRespModel>>
{
    private readonly AppDbContext _db;

    public CreateDonationHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.CreateDonationCommand reqModel, CancellationToken cancellationToken)
    {
        var service = new DonationService(_db);
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

        return await service.CreateDonation(donationRequest);
    }
}
