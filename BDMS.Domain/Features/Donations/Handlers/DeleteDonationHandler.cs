using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donations.Handlers;

public class DeleteDonationHandler : IRequestHandler<Commands.DeleteDonationCommand, Result<DonationRespModel>>
{
    private readonly AppDbContext _db;

    public DeleteDonationHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonationRespModel>> Handle(Commands.DeleteDonationCommand reqModel, CancellationToken cancellationToken)
    {
        var service = new DonationService(_db);
        return await service.DeleteDonation(reqModel.Id);
    }
}
