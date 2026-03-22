using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Queries;

public class GetAllDonorByIdQuery : IRequest<Result<DonorRespModel>>
{
    public int Id { get; set; }
}
