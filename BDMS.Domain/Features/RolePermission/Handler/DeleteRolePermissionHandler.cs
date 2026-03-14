using BDMS.Domain.Features.RolePermission.Command;
using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission.Handler
{
    public class DeleteRolePermissionHandler : IRequestHandler<Command.DeleteRolePermissionCommand, Result<RolePermissionReqRespModel>>
    {
        private readonly IRolePermissionService _rolePermissionService;

        public DeleteRolePermissionHandler(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        public async Task<Result<RolePermissionReqRespModel>> Handle(DeleteRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var rolePermission = new RolePermissionReqRespModel { RoleId = request.RoleId, PermissionId = request.PermissionId};
            return await _rolePermissionService.DeleteRolePermission(rolePermission);
        }
    }
}
