using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class GetAllDonorByIdHandler : IRequestHandler<Queries.GetAllDonorByIdQuery, Result<DonorRespModel>>
{
    private readonly IDonorService _donorService;

    public GetAllDonorByIdHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<DonorRespModel>> Handle(Queries.GetAllDonorByIdQuery request, CancellationToken cancellationToken)
    {
        return await _donorService.GetDonorById(request.Id);
    }
}
