using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donor.Handlers;

public class CreateDonorHandler : IRequestHandler<Commands.CreateDonorCommand, Result<DonorRespModel>>
{
    private readonly AppDbContext _db;

    public CreateDonorHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.CreateDonorCommand request, CancellationToken cancellationToken)
    {
        var service = new DonorService(_db);
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
        return await service.CreateDonor(donor_request);
    }
}
