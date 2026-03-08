using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donor.Commands;

public class CreateDonorCommand : IRequest<Result<DonorRespModel>>
{
    public int UserId { get; set; }

    public string NicNo { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; }

    public string BloodGroup { get; set; }

    public DateOnly? LastDonationDate { get; set; }

    public string? Remarks { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
