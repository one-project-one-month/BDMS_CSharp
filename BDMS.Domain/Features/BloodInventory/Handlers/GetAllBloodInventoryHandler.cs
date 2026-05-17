using BDMS.Domain.Features.BloodInventory.Commands;
using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Domain.Features.BloodInventory.Queries;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory.Handlers
{
    public class GetAllBloodInventoryHandler : IRequestHandler<GetAllBloodInventoryQuery, Result<List<BloodInventoryResModel>>>
    {
        private readonly IBloodInventoryService _service;
        public GetAllBloodInventoryHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<List<BloodInventoryResModel>>> Handle(
            GetAllBloodInventoryQuery request, CancellationToken ct)
        {
            return await _service.GetAll(ct);
        }

    }
}
