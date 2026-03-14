using BDMS.Domain.Features.Roles.Models;
using BDMS.Domain.Features.Roles.Queries;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles.Handlers
{
    public class GetAllRoleHandler:IRequestHandler<Queries.GetAllRoles, Result<List<RolesReqRespModel>>>
    {
        private readonly IRoleService _roleService;
        public GetAllRoleHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<List<RolesReqRespModel>>> Handle(GetAllRoles request, CancellationToken cancellationToken)
        {
            return await _roleService.GetAllRoles();
        }
    }
}
