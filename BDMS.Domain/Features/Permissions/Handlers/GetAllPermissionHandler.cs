using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Domain.Features.Permissions.Query;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class GetAllPermissionHandler: IRequestHandler<Query.GetAllPermissions, Result<List<PermissionReqRespModel>>>
    {
        private readonly AppDbContext _db;

        public GetAllPermissionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<PermissionReqRespModel>>> Handle(GetAllPermissions query, CancellationToken cancellationToken)
        {
            var service = new PermissionService(_db);

            return await service.GetAllPermissions();
        }
    }
}
