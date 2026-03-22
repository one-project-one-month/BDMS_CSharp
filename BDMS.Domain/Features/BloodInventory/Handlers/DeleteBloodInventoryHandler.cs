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
    public class DeleteBloodInventoryHandler
    {
        private readonly IBloodInventoryService _service;
        public DeleteBloodInventoryHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<string>> Handle(
            DeleteBloodInventoryCommand request, CancellationToken ct)
        {
            return await _service.Delete(request.Id, ct);
        }
    }
}
