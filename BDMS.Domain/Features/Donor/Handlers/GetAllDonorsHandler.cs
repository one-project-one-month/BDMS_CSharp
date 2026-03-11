using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class GetAllDonorsHandler : IRequestHandler<Queries.GetAllDonorsQuery, Result<List<DonorRespModel>>>
{
    private readonly AppDbContext _db;

    public GetAllDonorsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DonorRespModel>>> Handle(Queries.GetAllDonorsQuery request, CancellationToken cancellationToken)
    {
        var service = new DonorService(_db);
        return await service.GetAllDonors();
    }
}
