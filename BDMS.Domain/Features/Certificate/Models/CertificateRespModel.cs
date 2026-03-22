namespace BDMS.Domain.Features.Certificate.Models;

public class CertificateRespModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? CertificateTitle { get; set; }
    public string? CertificateDescription { get; set; }
    public string? CertificateData { get; set; }
    public DateOnly CreatedAt { get; set; }
}
