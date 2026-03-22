using BDMS.Domain.Features.Permissions.Models;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles
{
    public interface IRoleService
    {
        Task<Result<RolesReqRespModel>> CreateRole(RolesReqRespModel rolesReqRespModel);
        Task<Result<RolesReqRespModel>> DeleteRoleById(int Id);
        Task<Result<List<RolesReqRespModel>>> GetAllRoles();
        Task<Result<RolesReqRespModel>> UpdateRole(RolesReqRespModel rolesReqRespModel);
    }
}
