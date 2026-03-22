using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Certificate.Handlers;

public class GetCertificatesByDonorIdHandler : IRequestHandler<GetCertificatesByDonorIdQuery, Result<List<CertificateRespModel>>>
{
    private readonly ICertificateService _certificateService;

    public GetCertificatesByDonorIdHandler(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    public async Task<Result<List<CertificateRespModel>>> Handle(GetCertificatesByDonorIdQuery request, CancellationToken ct)
    {
        return await _certificateService.GetCertificatesByDonorId(request.DonorId, ct);
    }
}
