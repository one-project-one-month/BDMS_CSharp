using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Domain.Features.RolePermission.Query;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.RolePermission.Handler
{
    public class GetAllRolePermissionHandler : IRequestHandler<Query.GetAllRolePermission, Result<List<RolePermissionReqRespModel>>>
    {
        private readonly RolePermissionService _rolePermissionService;
        public GetAllRolePermissionHandler(RolePermissionService service) 
        {
            _rolePermissionService = service;
        }

        public async Task<Result<List<RolePermissionReqRespModel>>> Handle(GetAllRolePermission request, CancellationToken cancellationToken)
        {
            return await _rolePermissionService.GetAllRolePermissions();
        }
    }
}
