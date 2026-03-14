using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Handlers;

public class GenerateCertificateHandler : IRequestHandler<GenerateCertificateCommand, Result<CertificateRespModel>>
{
    private readonly ICertificateService _certificateService;

    public GenerateCertificateHandler(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    public async Task<Result<CertificateRespModel>> Handle(GenerateCertificateCommand request, CancellationToken ct)
    {
        return await _certificateService.GenerateCertificate(request, ct);
    }
}
