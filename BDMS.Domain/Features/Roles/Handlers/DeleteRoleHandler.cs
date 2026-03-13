using BDMS.Domain.Features.Roles.Commands;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles.Handlers
{
    public class DeleteRoleHandler : IRequestHandler<Commands.DeleteRoleCommand, Result<RolesReqRespModel>>
    {
        private readonly RoleService _roleService;
        public DeleteRoleHandler(RoleService roleService ) 
        {
            this._roleService = roleService;
        }

        public async Task<Result<RolesReqRespModel>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
        {
            var roleDelete = new RolesReqRespModel { Id = command.Id };
            return await _roleService.CreateRole(roleDelete);
        }
    }
}
