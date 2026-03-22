using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Models
{
    public class AvailableStockResModel
    {
        public int HospitalId { get; set; }
        public string BloodGroup { get; set; } = null!;
        public int TotalUnits { get; set; }
        public int AvailableCount { get; set; }
    }
}
