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
    public class UseFromInventoryHandler
    {
        private readonly IBloodInventoryService _service;
        public UseFromInventoryHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<BloodInventoryResModel>> Handle(
            UseFromInventoryCommand request, CancellationToken ct)
        {
            return await _service.UseFromInventory(request.BloodInventoryId, request.RequestId, ct);
        }
    }
}
