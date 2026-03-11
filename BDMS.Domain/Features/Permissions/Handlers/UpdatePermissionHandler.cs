using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Permissions.Commands;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class UpdatePermissionHandler : IRequestHandler<Commands.UpdatePermissionCommand, Result<PermissionReqRespModel>>
    {
        private readonly AppDbContext _db;
        public UpdatePermissionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            var service = new PermissionService(_db);

            var update_model = new PermissionReqRespModel{Id = request.Id, Name = request.Name};

            return await service.UpdatePermission(update_model);
        }
    }
}
