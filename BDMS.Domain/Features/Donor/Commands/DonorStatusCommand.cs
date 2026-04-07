using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Commands;

public class DonorStatusCommand : IRequest<Result<DonorRespModel>>
{
    public int DonorId { get; set; }
    public bool IsActive { get; set; }
}
