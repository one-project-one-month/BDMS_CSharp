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
    public class AddToInventoryHandler
    {
        private readonly IBloodInventoryService _service;
        public AddToInventoryHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<BloodInventoryResModel>> Handle(
            AddtoInventoryCommand request, CancellationToken ct)
        {
            return await _service.AddtoInventory(request.DonationId, ct);
        }
    }
}
