using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission.Query
{
    public class GetAllRolePermission:IRequest<Result<List<RolePermissionReqRespModel>>>
    {
    }
}
