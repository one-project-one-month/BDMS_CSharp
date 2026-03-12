using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class GetAllDonorsHandler : IRequestHandler<Queries.GetAllDonorsQuery, Result<List<DonorRespModel>>>
{
    private readonly IDonorService _donorService;

    public GetAllDonorsHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<List<DonorRespModel>>> Handle(Queries.GetAllDonorsQuery request, CancellationToken cancellationToken)
    {
        return await _donorService.GetAllDonors();
    }
}
