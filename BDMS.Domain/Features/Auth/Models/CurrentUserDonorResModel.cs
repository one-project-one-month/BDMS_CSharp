namespace BDMS.Domain.Features.Auth.Models
{
    public class CurrentUserDonorResModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string NicNo { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public string BloodGroup { get; set; } = null!;
        public DateOnly? LastDonationDate { get; set; }
        public string? Remarks { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public static CurrentUserDonorResModel? FromDatabase(BDMS.Database.AppDbContextModels.Donor? donor) =>
            donor == null
                ? null
                : new CurrentUserDonorResModel
                {
                    Id = donor.Id,
                    UserId = donor.UserId,
                    NicNo = donor.NicNo,
                    DateOfBirth = donor.DateOfBirth,
                    Gender = donor.Gender,
                    BloodGroup = donor.BloodGroup,
                    LastDonationDate = donor.LastDonationDate,
                    Remarks = donor.Remarks,
                    EmergencyContact = donor.EmergencyContact,
                    EmergencyPhone = donor.EmergencyPhone,
                    Address = donor.Address,
                    IsActive = donor.IsActive,
                    Status = donor.Status,
                    CreatedAt = donor.CreatedAt,
                    UpdatedAt = donor.UpdatedAt,
                    DeletedAt = donor.DeletedAt
                };
    }
}
