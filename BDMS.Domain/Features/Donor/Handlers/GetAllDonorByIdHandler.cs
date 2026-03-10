using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class GetAllDonorByIdHandler : IRequestHandler<Queries.GetAllDonorByIdQuery, Result<DonorRespModel>>
{
    private readonly AppDbContext _db;

    public GetAllDonorByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonorRespModel>> Handle(Queries.GetAllDonorByIdQuery request, CancellationToken cancellationToken)
    {
        var service = new DonorService(_db);
        return await service.GetDonorById(request.Id);
    }
}
