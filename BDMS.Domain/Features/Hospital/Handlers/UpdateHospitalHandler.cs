using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Hospital.Commands;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Domain.Features.Hospital.Queries;
using BDMS.Shared;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BDMS.Domain.Features.Hospital.Handlers;

public class UpdateHospitalHandler : IRequestHandler<Commands.UpdateHospitalCommand, Result<HospitalRespModel>>
{
    private readonly IHospitalService _hospitalService;

    public UpdateHospitalHandler(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    public async Task<Result<HospitalRespModel>> Handle(Commands.UpdateHospitalCommand reqModel, CancellationToken cancellationToken)
    {
        return await _hospitalService.UpdateHospital(reqModel);
    }
}
