using BDMS.Domain.Features.Hospital.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Hospital.Queries;

public class GetAllHospitalsQuery : IRequest<Result<List<HospitalRespModel>>>
{
}
