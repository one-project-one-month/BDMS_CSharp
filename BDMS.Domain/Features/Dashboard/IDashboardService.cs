using BDMS.Shared;
using System;
namespace BDMS.Domain.Features.Dashboard
{
    public interface IDashboardService
    {
        Task<Result<string>> GetDashboardById(int id, CancellationToken ct);
    }
}
