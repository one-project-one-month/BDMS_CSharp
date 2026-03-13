using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using CertificateEntity = BDMS.Database.AppDbContextModels.Certificate;

namespace BDMS.Domain.Features.Certificate;

public class CertificateService : ICertificateService
{
    private readonly AppDbContext _db;

    public CertificateService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CertificateRespModel>> GenerateCertificate(GenerateCertificateCommand request, CancellationToken ct)
    {
        try
        {
            var donor = await _db.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Certificate.DonorId && x.DeletedAt == null && x.IsActive, ct);

            if (donor is null)
                return Result<CertificateRespModel>.ValidationError("Donor not found or is inactive");

            // Check if donor has at least one completed donation
            var hasCompletedDonation = await _db.Appointments
                .AnyAsync(x => x.DonationId != null && 
                               x.UserId == donor.UserId && 
                               x.Status.ToLower() == EnumAppointmentStatus.Completed.ToString().ToLower() && 
                               x.DeletedAt == null, ct);

            if (!hasCompletedDonation)
                return Result<CertificateRespModel>.ValidationError("Donor must have at least one completed donation to generate a certificate");

            var certificate = new CertificateEntity()
            {
                UserId = donor.UserId,
                CertificateTitle = request.Certificate.CertificateTitle ?? "Donation Certificate",
                CertificateDescription = request.Certificate.CertificateDescription ?? $"Certificate for blood donor {donor.NicNo}",
                CertificateData = Guid.NewGuid().ToString(), // Placeholder for certificate data/link
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _db.Certificates.Add(certificate);
            await _db.SaveChangesAsync(ct);

            var response = new CertificateRespModel()
            {
                Id = certificate.Id,
                UserId = certificate.UserId,
                CertificateTitle = certificate.CertificateTitle,
                CertificateDescription = certificate.CertificateDescription,
                CertificateData = certificate.CertificateData,
                CreatedAt = certificate.CreatedAt
            };

            return Result<CertificateRespModel>.Success(response, "Certificate generated successfully");
        }
        catch (Exception ex)
        {
            return Result<CertificateRespModel>.SystemError($"Error generating certificate: {ex.Message}");
        }
    }

    public async Task<Result<CertificateRespModel>> GetCertificateById(GetCertificateByIdQuery request, CancellationToken ct)
    {
        try
        {
            var certificate = await _db.Certificates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (certificate is null)
                return Result<CertificateRespModel>.NotFound("Certificate not found");

            var response = new CertificateRespModel()
            {
                Id = certificate.Id,
                UserId = certificate.UserId,
                CertificateTitle = certificate.CertificateTitle,
                CertificateDescription = certificate.CertificateDescription,
                CertificateData = certificate.CertificateData,
                CreatedAt = certificate.CreatedAt
            };

            return Result<CertificateRespModel>.Success(response, "Success");
        }
        catch (Exception ex)
        {
            return Result<CertificateRespModel>.SystemError($"Error retrieving certificate: {ex.Message}");
        }
    }

    public async Task<Result<List<CertificateRespModel>>> GetCertificatesByDonorId(int donorId, CancellationToken ct)
    {
        try
        {
            var donor = await _db.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == donorId && x.DeletedAt == null, ct);

            if (donor is null)
                return Result<List<CertificateRespModel>>.NotFound("Donor not found");

            var certificates = await _db.Certificates
                .Where(x => x.UserId == donor.UserId)
                .AsNoTracking()
                .Select(x => new CertificateRespModel()
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    CertificateTitle = x.CertificateTitle,
                    CertificateDescription = x.CertificateDescription,
                    CertificateData = x.CertificateData,
                    CreatedAt = x.CreatedAt
                }).ToListAsync(ct);

            return Result<List<CertificateRespModel>>.Success(certificates, "Success");
        }
        catch (Exception ex)
        {
            return Result<List<CertificateRespModel>>.SystemError($"Error retrieving certificates: {ex.Message}");
        }
    }
}
