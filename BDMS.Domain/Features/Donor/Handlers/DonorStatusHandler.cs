using BDMS.Domain.Features.Donor.Commands;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class DonorStatusHandler : IRequestHandler<DonorStatusCommand, Result<DonorRespModel>>
{
    private readonly IDonorService _donorService;

    public DonorStatusHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<DonorRespModel>> Handle(DonorStatusCommand request, CancellationToken cancellationToken)
    {
        return await _donorService.UpdateDonorStatus(request.DonorId, request.IsActive);
    }
}
