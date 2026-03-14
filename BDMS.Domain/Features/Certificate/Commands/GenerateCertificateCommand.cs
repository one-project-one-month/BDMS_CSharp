using BDMS.Domain.Features.Certificate.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Commands;

public class GenerateCertificateCommand : IRequest<Result<CertificateRespModel>>
{
    public CertificateReqModel Certificate { get; set; } = null!;
}
