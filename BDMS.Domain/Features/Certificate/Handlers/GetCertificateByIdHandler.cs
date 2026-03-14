using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Handlers;

public class GetCertificateByIdHandler : IRequestHandler<GetCertificateByIdQuery, Result<CertificateRespModel>>
{
    private readonly ICertificateService _certificateService;

    public GetCertificateByIdHandler(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    public async Task<Result<CertificateRespModel>> Handle(GetCertificateByIdQuery request, CancellationToken ct)
    {
        return await _certificateService.GetCertificateById(request, ct);
    }
}
