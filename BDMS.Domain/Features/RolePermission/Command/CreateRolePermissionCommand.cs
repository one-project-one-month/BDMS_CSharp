using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission.Command
{
    public class CreateRolePermissionCommand : IRequest<Result<RolePermissionReqRespModel>>
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}
