using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Donor.Commands;

public class BecomeDonorCommand : IRequest<Result<DonorRespModel>>
{
    public int UserId { get; set; }

    public string NicNo { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string BloodGroup { get; set; } = string.Empty;

    public DateOnly? LastDonationDate { get; set; }

    public string? Remarks { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? Address { get; set; }
}
