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
    public class GetInventoryByHospitalHandler
    {
        private readonly IBloodInventoryService _service;
        public GetInventoryByHospitalHandler(IBloodInventoryService service)
        {
            _service = service;
        }
        public async Task<Result<List<BloodInventoryResModel>>> Handle(
            GetInventoryByHospitalQuery request, CancellationToken ct)
        {
            return await _service.GetByHospital(request.HospitalId,ct);
        }
    }
}
