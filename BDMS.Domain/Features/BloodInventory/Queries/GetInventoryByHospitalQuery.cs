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
    public class GetInventoryByHospitalQuery : IRequest<Result<List<BloodInventoryResModel>>>
    {
        public int HospitalId { get; set; }
    }
}
