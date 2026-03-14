using BDMS.Database.AppDbContextModels;
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

public class UpdateDonationHandler : IRequestHandler<Commands.UpdateDonationCommand, Result<DonationRespModel>>
{
    private readonly AppDbContext _db;

    public UpdateDonationHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.UpdateDonationCommand reqModel, CancellationToken cancellationToken)
    {
        var service = new DonationService(_db);

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
        return await service.UpdateDonation(donationRequest);
    }
}

