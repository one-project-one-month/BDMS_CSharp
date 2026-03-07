using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donation.Handlers;

public class GetAllDonationHandler : IRequestHandler<Queries.GetAllDonationQuery, Result<List<DonationRespModel>>>
{
    private readonly AppDbContext _db;

    public GetAllDonationHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<DonationRespModel>>> Handle(Queries.GetAllDonationQuery query, CancellationToken cancellationToken)
    {
        var service = new DonationService(_db);
        return await service.GetAllDonationsAsync();
    }
}
