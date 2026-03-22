using BDMS.Domain.Features.BloodInventory.Commands;
using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Handlers
{
    public class RunStockTakeHandler
    {
        private readonly IBloodInventoryService _service;
        public RunStockTakeHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<int>> Handle(
            RunStockTakeCommand request, CancellationToken ct)
        {
            return await _service.RunStockTake(request.HospitalId, ct);
        }
    }
}
