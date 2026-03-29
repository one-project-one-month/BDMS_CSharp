using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Domain.Features.Hospital.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Hospital.Handlers;

public class GetHospitalByIdHandler : IRequestHandler<Queries.GetHospitalByIdQuery, Result<HospitalRespModel>>
{
    private readonly IHospitalService _hospitalService;

    public GetHospitalByIdHandler(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    public async Task<Result<HospitalRespModel>> Handle(Queries.GetHospitalByIdQuery request, CancellationToken cancellationToken)
    {
        return await _hospitalService.GetHospitalById(request.Id);
    }
}
