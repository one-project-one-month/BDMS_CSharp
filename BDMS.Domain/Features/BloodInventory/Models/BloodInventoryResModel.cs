using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Models
{
    public class BloodInventoryResModel
    {
        public int Id { get; set; }
        public int DonationId { get; set; }
        public int HospitalId { get; set; }
        public string BloodGroup { get; set; } = null!;
        public int Units { get; set; }
        public DateOnly? CollectedAt { get; set; }
        public DateOnly? ExpiredAt { get; set; }
        public string Status { get; set; } = null!;
        public int? RequestId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
