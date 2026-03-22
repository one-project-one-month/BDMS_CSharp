using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.BloodInventory
{
    public interface IBloodInventoryService
    {
        Task<Result<BloodInventoryResModel>> AddtoInventory(int donationId, CancellationToken cancellationToken);
        Task<Result<BloodInventoryResModel>> UseFromInventory(int inventoryId, int requestId, CancellationToken cancellationToken);
        Task<Result<int>> RunStockTake(int? hospitalId, CancellationToken cancellationToken);
        Task<Result<List<BloodInventoryResModel>>> GetAll(CancellationToken ct);
        Task<Result<BloodInventoryResModel>> GetById(int id, CancellationToken ct);
        Task<Result<List<BloodInventoryResModel>>> GetByHospital(int hospitalId, CancellationToken ct);
        Task<Result<List<AvailableStockResModel>>> GetAvailableStock(int? hospitalId, string? bloodGroup, CancellationToken ct);
        Task<Result<string>> Delete(int id, CancellationToken ct);
    }
}
