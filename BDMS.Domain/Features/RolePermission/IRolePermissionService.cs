using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission
{
    public interface IRolePermissionService
    {
        Task<Result<RolePermissionReqRespModel>> CreateRolePermission(RolePermissionReqRespModel rolePermissionReqRespModel);
        Task<Result<RolePermissionReqRespModel>> DeleteRolePermission(RolePermissionReqRespModel rolePermissionReqRespModel);
        Task<Result<List<RolePermissionReqRespModel>>> GetAllRolePermissions();
        Task<Result<RolePermissionReqRespModel>> UpdateRolePermission(RolePermissionReqRespModel oldModel, RolePermissionReqRespModel newModel);
    }
}
