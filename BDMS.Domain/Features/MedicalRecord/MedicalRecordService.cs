using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.MedicalRecord;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly AppDbContext _db;
    private static readonly string[] AllowedResults = ["positive", "negative", "inconclusive"];
    private static readonly string[] AllowedScreenStatuses = ["pending", "failed", "passed"];

    public MedicalRecordService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<MedicalRecordRespModel>>> GetAll(CancellationToken ct)
    {
        try
        {
            var records = await _db.MedicalRecords
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return Result<List<MedicalRecordRespModel>>.Success(records.Select(ToResponse).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<MedicalRecordRespModel>>.SystemError($"Error retrieving medical records: {ex.Message}");
        }
    }

    public async Task<Result<MedicalRecordRespModel>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var record = await _db.MedicalRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);

            if (record == null)
                return Result<MedicalRecordRespModel>.NotFound("Medical record not found.");

            return Result<MedicalRecordRespModel>.Success(ToResponse(record));
        }
        catch (Exception ex)
        {
            return Result<MedicalRecordRespModel>.SystemError($"Error retrieving medical record: {ex.Message}");
        }
    }

    public async Task<Result<MedicalRecordRespModel>> Create(CreateMedicalRecordCommand command, CancellationToken ct)
    {
        var validation = Validate(command.HivResult, command.HepatitisBResult, command.HepatitisCResult,
            command.MalariaResult, command.SyphilisResult, command.ScreeningStatus);
        if (validation != null)
            return Result<MedicalRecordRespModel>.ValidationError(validation);

        try
        {
            var donation = await _db.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.DonationId && x.DeletedAt == null, ct);

            if (donation == null)
                return Result<MedicalRecordRespModel>.NotFound("Donation not found.");

            var record = new Database.AppDbContextModels.MedicalRecord
            {
                DonationId = command.DonationId,
                HospitalId = donation.HospitalId,
                HemoglobinLevel = command.HemoglobinLevel,
                HivResult = Normalize(command.HivResult),
                HepatitisBResult = Normalize(command.HepatitisBResult),
                HepatitisCResult = Normalize(command.HepatitisCResult),
                MalariaResult = Normalize(command.MalariaResult),
                SyphilisResult = Normalize(command.SyphilisResult),
                ScreeningStatus = Normalize(command.ScreeningStatus),
                ScreeningNotes = command.ScreeningNotes,
                ScreenedBy = command.ScreenedBy,
                ScreeningAt = command.ScreeningAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.MedicalRecords.Add(record);
            await _db.SaveChangesAsync(ct);

            return Result<MedicalRecordRespModel>.Success(ToResponse(record), "Medical record created successfully.");
        }
        catch (Exception ex)
        {
            return Result<MedicalRecordRespModel>.SystemError($"Error creating medical record: {ex.Message}");
        }
    }

    public async Task<Result<MedicalRecordRespModel>> Update(UpdateMedicalRecordCommand command, CancellationToken ct)
    {
        var validation = Validate(command.HivResult, command.HepatitisBResult, command.HepatitisCResult,
            command.MalariaResult, command.SyphilisResult, command.ScreeningStatus);
        if (validation != null)
            return Result<MedicalRecordRespModel>.ValidationError(validation);

        try
        {
            var record = await _db.MedicalRecords
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null, ct);

            if (record == null)
                return Result<MedicalRecordRespModel>.NotFound("Medical record not found.");

            var donation = await _db.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.DonationId && x.DeletedAt == null, ct);

            if (donation == null)
                return Result<MedicalRecordRespModel>.NotFound("Donation not found.");

            record.DonationId = command.DonationId;
            record.HospitalId = donation.HospitalId;
            record.HemoglobinLevel = command.HemoglobinLevel;
            record.HivResult = Normalize(command.HivResult);
            record.HepatitisBResult = Normalize(command.HepatitisBResult);
            record.HepatitisCResult = Normalize(command.HepatitisCResult);
            record.MalariaResult = Normalize(command.MalariaResult);
            record.SyphilisResult = Normalize(command.SyphilisResult);
            record.ScreeningStatus = Normalize(command.ScreeningStatus);
            record.ScreeningNotes = command.ScreeningNotes;
            record.ScreenedBy = command.ScreenedBy;
            record.ScreeningAt = command.ScreeningAt;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Result<MedicalRecordRespModel>.Success(ToResponse(record), "Medical record updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<MedicalRecordRespModel>.SystemError($"Error updating medical record: {ex.Message}");
        }
    }

    public async Task<Result<string>> Delete(int id, CancellationToken ct)
    {
        try
        {
            var record = await _db.MedicalRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);

            if (record == null)
                return Result<string>.NotFound("Medical record not found.");

            record.DeletedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Result<string>.DeleteSuccess();
        }
        catch (Exception ex)
        {
            return Result<string>.SystemError($"Error deleting medical record: {ex.Message}");
        }
    }

    private static string? Validate(string? hiv, string? hepB, string? hepC, string? malaria, string? syphilis, string? screeningStatus)
    {
        if (!IsValueAllowed(hiv, AllowedResults)) return "Invalid hiv result.";
        if (!IsValueAllowed(hepB, AllowedResults)) return "Invalid hepatitis b result.";
        if (!IsValueAllowed(hepC, AllowedResults)) return "Invalid hepatitis c result.";
        if (!IsValueAllowed(malaria, AllowedResults)) return "Invalid malaria result.";
        if (!IsValueAllowed(syphilis, AllowedResults)) return "Invalid syphilis result.";
        if (!IsValueAllowed(screeningStatus, AllowedScreenStatuses)) return "Invalid screening status.";

        return null;
    }

    private static bool IsValueAllowed(string? value, string[] allowed)
        => string.IsNullOrWhiteSpace(value) || allowed.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static MedicalRecordRespModel ToResponse(Database.AppDbContextModels.MedicalRecord record) => new()
    {
        Id = record.Id,
        DonationId = record.DonationId,
        HospitalId = record.HospitalId,
        HemoglobinLevel = record.HemoglobinLevel,
        HivResult = record.HivResult,
        HepatitisBResult = record.HepatitisBResult,
        HepatitisCResult = record.HepatitisCResult,
        MalariaResult = record.MalariaResult,
        SyphilisResult = record.SyphilisResult,
        ScreeningStatus = record.ScreeningStatus,
        ScreeningNotes = record.ScreeningNotes,
        ScreenedBy = record.ScreenedBy,
        ScreeningAt = record.ScreeningAt,
        CreatedAt = record.CreatedAt,
        UpdatedAt = record.UpdatedAt,
        DeletedAt = record.DeletedAt
    };
}
