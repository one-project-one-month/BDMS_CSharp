using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;

namespace BDMS.Domain.Features.Certificate;

public interface ICertificateService
{
    Task<Result<CertificateRespModel>> GenerateCertificate(GenerateCertificateCommand request, CancellationToken ct);
    Task<Result<CertificateRespModel>> GetCertificateById(GetCertificateByIdQuery request, CancellationToken ct);
    Task<Result<List<CertificateRespModel>>> GetCertificatesByDonorId(int donorId, CancellationToken ct);
}
