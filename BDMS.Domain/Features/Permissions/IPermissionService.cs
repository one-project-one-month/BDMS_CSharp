using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Permissions
{
    public interface IPermissionService
    {
        Task<Result<PermissionReqRespModel>> CreatePermission(PermissionReqRespModel permissionReqRespModel);
        Task<Result<PermissionReqRespModel>> DeletePermissionById(int Id);
        Task<Result<List<PermissionReqRespModel>>> GetAllPermissions();
        Task<Result<PermissionReqRespModel>> UpdatePermission(PermissionReqRespModel permissionReqRespModel);
    }
}