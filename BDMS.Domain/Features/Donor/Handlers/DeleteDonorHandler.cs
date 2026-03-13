using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Handlers;

public class DeleteDonorHandler : IRequestHandler<Commands.DeleteDonorCommand, Result<DonorRespModel>>
{
    private readonly IDonorService _donorService;

    public DeleteDonorHandler(IDonorService donorService)
    {
        _donorService = donorService;
    }

    public async Task<Result<DonorRespModel>> Handle(Commands.DeleteDonorCommand request, CancellationToken cancellationToken)
    {
        return await _donorService.DeleteDonor(request.Id);
    }
}
