namespace BDMS.Domain.Features.Certificate.Models;

public class CertificateReqModel
{
    public int DonorId { get; set; }
    public string? CertificateTitle { get; set; }
    public string? CertificateDescription { get; set; }
}
