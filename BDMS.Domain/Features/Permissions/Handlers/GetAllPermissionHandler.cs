using BDMS.Domain.Features.Permissions.Models;
using BDMS.Domain.Features.Permissions.Query;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class GetAllPermissionHandler: IRequestHandler<Query.GetAllPermissions, Result<List<PermissionReqRespModel>>>
    {
        private readonly IPermissionService _permissionService;

        public GetAllPermissionHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<Result<List<PermissionReqRespModel>>> Handle(GetAllPermissions query, CancellationToken cancellationToken)
        {
            return await _permissionService.GetAllPermissions();
        }
    }
}
