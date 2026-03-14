using BDMS.Domain.Features.Roles.Commands;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BDMS.Domain.Features.Roles.Handlers
{
    public class CreateRoleHandler : IRequestHandler<Commands.CreateRoleCommand, Result<RolesReqRespModel>>
    {
        private readonly RoleService _roleService;

        public CreateRoleHandler(RoleService roleService)
        {
            this._roleService = roleService;
        }
        public async Task<Result<RolesReqRespModel>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var roleCreate = new RolesReqRespModel { Id = request.Id, Name = request.Name };

            return await _roleService.CreateRole(roleCreate);
        }
    }
}
