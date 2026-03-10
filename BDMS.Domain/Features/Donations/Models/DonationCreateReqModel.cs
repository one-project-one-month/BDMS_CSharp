using BDMS.Database.AppDbContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Donations.Models;

public class DonationCreateReqModel
{
    public int DonorId { get; set; }

    public int HospitalId { get; set; }

    public int? BloodRequestId { get; set; }

    public int CreatedBy { get; set; }

    public string? DonationCode { get; set; }

    public string BloodGroup { get; set; } = null!;

    public int? UnitsDonated { get; set; }

    public DateOnly? DonationDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

}
