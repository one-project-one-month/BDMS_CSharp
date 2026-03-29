using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Hospital.Handlers;

public class GetAllHospitalsHandler : IRequestHandler<Queries.GetAllHospitalsQuery, Result<List<HospitalRespModel>>>
{
    private readonly IHospitalService _hospitalService;

    public GetAllHospitalsHandler(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    public async Task<Result<List<HospitalRespModel>>> Handle(Queries.GetAllHospitalsQuery request, CancellationToken cancellationToken)
    {
        return await _hospitalService.GetAllHospitals();
    }
}
