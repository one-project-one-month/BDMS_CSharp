using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Shared;
using BDMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Domain.Features.MedicalRecord;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly AppDbContext _db;
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
        var enumValidation = ValidateEnums(command.HivResult, command.HepatitisBResult, command.HepatitisCResult,
            command.MalariaResult, command.SyphilisResult, command.ScreeningStatus);
        if (enumValidation != null)
            return Result<MedicalRecordRespModel>.ValidationError(enumValidation);

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
                HivResult = command.HivResult.ToDatabaseValue(),
                HepatitisBResult = command.HepatitisBResult.ToDatabaseValue(),
                HepatitisCResult = command.HepatitisCResult.ToDatabaseValue(),
                MalariaResult = command.MalariaResult.ToDatabaseValue(),
                SyphilisResult = command.SyphilisResult.ToDatabaseValue(),
                ScreeningStatus = command.ScreeningStatus.ToDatabaseValue(),
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
        var enumValidation = ValidateEnums(command.HivResult, command.HepatitisBResult, command.HepatitisCResult,
            command.MalariaResult, command.SyphilisResult, command.ScreeningStatus);
        if (enumValidation != null)
            return Result<MedicalRecordRespModel>.ValidationError(enumValidation);

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
            record.HivResult = command.HivResult.ToDatabaseValue();
            record.HepatitisBResult = command.HepatitisBResult.ToDatabaseValue();
            record.HepatitisCResult = command.HepatitisCResult.ToDatabaseValue();
            record.MalariaResult = command.MalariaResult.ToDatabaseValue();
            record.SyphilisResult = command.SyphilisResult.ToDatabaseValue();
            record.ScreeningStatus = command.ScreeningStatus.ToDatabaseValue();
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

    private static string? ValidateEnums(
        EnumMedicalRecordResult hiv,
        EnumMedicalRecordResult hepatitisB,
        EnumMedicalRecordResult hepatitisC,
        EnumMedicalRecordResult malaria,
        EnumMedicalRecordResult syphilis,
        EnumMedicalRecordScreeningStatus screeningStatus)
    {
        if (!IsDefinedAndProvided(hiv)) return "Invalid hiv result.";
        if (!IsDefinedAndProvided(hepatitisB)) return "Invalid hepatitis b result.";
        if (!IsDefinedAndProvided(hepatitisC)) return "Invalid hepatitis c result.";
        if (!IsDefinedAndProvided(malaria)) return "Invalid malaria result.";
        if (!IsDefinedAndProvided(syphilis)) return "Invalid syphilis result.";
        if (!IsDefinedAndProvided(screeningStatus)) return "Invalid screening status.";

        return null;
    }

    private static bool IsDefinedAndProvided(EnumMedicalRecordResult value)
        => Enum.IsDefined(value) && value != EnumMedicalRecordResult.None;

    private static bool IsDefinedAndProvided(EnumMedicalRecordScreeningStatus value)
        => Enum.IsDefined(value) && value != EnumMedicalRecordScreeningStatus.None;

    private static MedicalRecordRespModel ToResponse(Database.AppDbContextModels.MedicalRecord record) => new()
    {
        Id = record.Id,
        DonationId = record.DonationId,
        HospitalId = record.HospitalId,
        HemoglobinLevel = record.HemoglobinLevel,
        HivResult = record.HivResult.ToEnumOrDefault(EnumMedicalRecordResult.None),
        HepatitisBResult = record.HepatitisBResult.ToEnumOrDefault(EnumMedicalRecordResult.None),
        HepatitisCResult = record.HepatitisCResult.ToEnumOrDefault(EnumMedicalRecordResult.None),
        MalariaResult = record.MalariaResult.ToEnumOrDefault(EnumMedicalRecordResult.None),
        SyphilisResult = record.SyphilisResult.ToEnumOrDefault(EnumMedicalRecordResult.None),
        ScreeningStatus = record.ScreeningStatus.ToEnumOrDefault(EnumMedicalRecordScreeningStatus.None),
        ScreeningNotes = record.ScreeningNotes,
        ScreenedBy = record.ScreenedBy,
        ScreeningAt = record.ScreeningAt,
        CreatedAt = record.CreatedAt,
        UpdatedAt = record.UpdatedAt,
        DeletedAt = record.DeletedAt
    };
}
