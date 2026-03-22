using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Commands
{
    public class RunStockTakeCommand : IRequest<Result<int>>
    {
        public int? HospitalId { get; set; }
    }
}
