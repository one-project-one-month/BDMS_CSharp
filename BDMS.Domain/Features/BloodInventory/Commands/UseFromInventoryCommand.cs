using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Commands
{
    public class UseFromInventoryCommand : IRequest<Result<AvailableStockResModel>> 
    {
        public int BloodInventoryId { get; set; }
        public int RequestId { get; set; }
    }
}
