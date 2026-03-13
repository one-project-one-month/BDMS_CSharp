using BDMS.Domain.Features.Certificate.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Queries;

public class GetCertificateByIdQuery : IRequest<Result<CertificateRespModel>>
{
    public int Id { get; set; }
}
