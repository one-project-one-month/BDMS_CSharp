using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class UpdateDonorHandler : IRequestHandler<Commands.UpdateDonorCommand, Result<Models.DonorRespModel>>
{
    private readonly AppDbContext _db;

    public UpdateDonorHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.UpdateDonorCommand command, CancellationToken cancellationToken)
    {
        var service = new DonorService(_db);
        var donor_request = new DonorReqModel()
        {
            Id = command.Id,
            UserId = command.UserId,
            NicNo = command.NicNo,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            BloodGroup = command.BloodGroup,
            LastDonationDate = command.LastDonationDate,
            Remarks = command.Remarks,
            EmergencyContact = command.EmergencyContact,
            EmergencyPhone = command.EmergencyPhone,
            Address = command.Address,
            IsActive = command.IsActive

        };
        return await service.UpdateDonor(donor_request);
    }
}
