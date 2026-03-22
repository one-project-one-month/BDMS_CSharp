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
    public class UpdateRolePermissionCommand : IRequest<Result<RolePermissionReqRespModel>>
    {
        public int oldRoleId { get; set; }
        public int newRoleId { get; set; }

        public int oldPermissionId { get; set; }
        public int newPermissionId { get; set; }
    }
}
