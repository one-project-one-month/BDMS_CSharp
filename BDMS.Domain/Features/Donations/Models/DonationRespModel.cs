using M = BDMS.Database.AppDbContextModels;

namespace BDMS.Domain.Features.Donation.Models;

public class DonationRespModel
{
    public int Id { get; set; }

    public int DonorId { get; set; }

    public int HospitalId { get; set; }

    public int? BloodRequestId { get; set; }

    public int CreatedBy { get; set; }

    public string? DonationCode { get; set; }

    public string BloodGroup { get; set; } = null!;

    public int? UnitsDonated { get; set; }

    public DateOnly? DonationDate { get; set; }

    public string Status { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<M.Appointment> Appointments { get; set; } = new List<M.Appointment>();

    public virtual M.User? ApprovedByNavigation { get; set; }

    public virtual M.BloodInventory? BloodInventory { get; set; }

    public virtual M.BloodRequest? BloodRequest { get; set; }

    public virtual M.User CreatedByNavigation { get; set; } = null!;

    public virtual M.Donor Donor { get; set; } = null!;

    public virtual M.Hospital Hospital { get; set; } = null!;

    public virtual M.MedicalRecord? MedicalRecord { get; set; }
}
