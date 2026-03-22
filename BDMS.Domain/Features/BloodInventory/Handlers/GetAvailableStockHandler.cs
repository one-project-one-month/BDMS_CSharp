using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Domain.Features.BloodInventory.Queries;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Handlers
{
    public class GetAvailableStockHandler
    {
        private readonly IBloodInventoryService _service;
        public GetAvailableStockHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<List<AvailableStockResModel>>> Handle(
            GetAvailableStockQuery request, CancellationToken ct)
        {
            return await _service.GetAvailableStock(request.HospitalId, request.BloodGroup,ct);
        }
    }
}
