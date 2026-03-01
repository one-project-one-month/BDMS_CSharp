using System;
using System.Collections.Generic;

namespace BDMS.Database.AppDbContextModels;

public partial class MedicalRecord
{
    public int Id { get; set; }

    public int DonationId { get; set; }

    public int HospitalId { get; set; }

    public decimal? HemoglobinLevel { get; set; }

    public string? HivResult { get; set; }

    public string? HepatitisBResult { get; set; }

    public string? HepatitisCResult { get; set; }

    public string? MalariaResult { get; set; }

    public string? SyphilisResult { get; set; }

    public string? ScreeningStatus { get; set; }

    public string? ScreeningNotes { get; set; }

    public int? ScreenedBy { get; set; }

    public DateTime? ScreeningAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Donation Donation { get; set; } = null!;

    public virtual Hospital Hospital { get; set; } = null!;

    public virtual User? ScreenedByNavigation { get; set; }
}
