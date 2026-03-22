using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Queries
{
    public class GetAvailableStockQuery : IRequest<Result<List<AvailableStockResModel>>>
    {
        public int? HospitalId { get; set; }
        public string? BloodGroup { get; set; }
    }
}
