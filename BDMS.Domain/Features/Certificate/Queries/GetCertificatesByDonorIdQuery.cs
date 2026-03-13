using BDMS.Domain.Features.Certificate.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Queries;

public class GetCertificatesByDonorIdQuery : IRequest<Result<List<CertificateRespModel>>>
{
    public int DonorId { get; set; }
}
