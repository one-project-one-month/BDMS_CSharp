using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class BecomeDonorHandler : IRequestHandler<Commands.BecomeDonorCommand, Result<DonorRespModel>>
{
    private readonly IDonorService _donorService;

    public BecomeDonorHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.BecomeDonorCommand request, CancellationToken cancellationToken)
    {
        var reqModel = new DonorReqModel
        {
            UserId = request.UserId,
            NicNo = request.NicNo,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            BloodGroup = request.BloodGroup,
            LastDonationDate = request.LastDonationDate,
            Remarks = request.Remarks,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            Address = request.Address
        };
        return await _donorService.BecomeDonor(request.UserId, reqModel);
    }
}
