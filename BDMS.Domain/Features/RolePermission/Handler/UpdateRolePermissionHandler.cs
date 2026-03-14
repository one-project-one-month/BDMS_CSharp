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
    public class UpdateRolePermissionHandler:IRequestHandler<Command.UpdateRolePermissionCommand, Result<RolePermissionReqRespModel>>
    {
        private readonly IRolePermissionService _rolePermissionService;
        public UpdateRolePermissionHandler(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        public async Task<Result<RolePermissionReqRespModel>> Handle(UpdateRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var oldModel = new RolePermissionReqRespModel { RoleId = request.oldRoleId, PermissionId = request.oldPermissionId };
            var newModel = new RolePermissionReqRespModel { RoleId = request.newRoleId, PermissionId = request.newPermissionId };

            return await _rolePermissionService.UpdateRolePermission(oldModel, newModel);
        }
    }
}
