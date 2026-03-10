using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class CreatePermissionHandler: IRequestHandler<Commands.CreatePermissionCommand, Result<PermissionReqRespModel>>
    {
        private readonly AppDbContext _db;

        public CreatePermissionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(Commands.CreatePermissionCommand command, CancellationToken cancellationToken)
        {
            var service = new PermissionService(_db);

            var permissionCreate = new PermissionReqRespModel { Id = command.Id, Name = command.Name};

            return await service.CreatePermission(permissionCreate);
        }
    }
}
