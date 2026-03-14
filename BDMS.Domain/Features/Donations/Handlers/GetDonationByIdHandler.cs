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

public class GetDonationByIdHandler : IRequestHandler<Queries.GetDonationByIdQuery, Result<DonationRespModel>>
{
    private readonly AppDbContext _db;

    public GetDonationByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DonationRespModel>> Handle(Queries.GetDonationByIdQuery query, CancellationToken cancellationToken)
    {
        var service = new DonationService(_db);
        return await service.GetDonationById(query.Id);
    }
}
