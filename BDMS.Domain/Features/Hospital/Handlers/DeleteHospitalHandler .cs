using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Domain.Features.Hospital.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Hospital.Handlers;

public class DeleteHospitalHandler : IRequestHandler<Commands.DeleteHospitalCommand, Result<HospitalRespModel>>
{
    private readonly IHospitalService _hospitalService;

    public DeleteHospitalHandler(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    public async Task<Result<HospitalRespModel>> Handle(Commands.DeleteHospitalCommand request, CancellationToken cancellationToken)
    {
        return await _hospitalService.DeleteHospital(request.Id);
    }
}
