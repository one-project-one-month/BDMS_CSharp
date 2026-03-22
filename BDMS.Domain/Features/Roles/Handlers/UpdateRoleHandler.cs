using Azure.Core;
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
    public class UpdateRoleHandler : IRequestHandler<Commands.UpdateRoleCommand, Result<RolesReqRespModel>>
    {
        private readonly IRoleService _roleService;

        public UpdateRoleHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<RolesReqRespModel>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var roleUpdate = new RolesReqRespModel { Id = request.Id, Name = request.Name };
            return await _roleService.UpdateRole(roleUpdate);
        }
    }
}
