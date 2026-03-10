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
    public class DeletePermissionHandler : IRequestHandler<Commands.DeletePermissionCommand, Result<PermissionReqRespModel>>
    {

        private readonly AppDbContext _db;
        public DeletePermissionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            var service = new PermissionService(_db);

            return await service.DeletePermissionById(request.Id);
        }
    }
}
