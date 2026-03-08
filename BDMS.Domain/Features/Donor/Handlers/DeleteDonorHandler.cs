using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class DeleteDonorHandler : IRequestHandler<Commands.DeleteDonorCommand, Result<DonorRespModel>>
{
    private readonly AppDbContext _db;

    public DeleteDonorHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.DeleteDonorCommand request, CancellationToken cancellationToken)
    {
        var service = new DonorService(_db);
        return await service.DeleteDonor(request.Id);
    }
}
