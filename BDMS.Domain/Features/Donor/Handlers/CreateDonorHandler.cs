using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class CreateDonorHandler : IRequestHandler<Commands.CreateDonorCommand, Result<DonorRespModel>>
{
    private readonly IDonorService _donorService;

    public CreateDonorHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var donor_request = new DonorReqModel()
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
            Address = request.Address,
            IsActive = request.IsActive
        };
        return await _donorService.CreateDonor(donor_request);
    }
}
