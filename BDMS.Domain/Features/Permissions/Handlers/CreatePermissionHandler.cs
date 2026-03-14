using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class CreatePermissionHandler: IRequestHandler<Commands.CreatePermissionCommand, Result<PermissionReqRespModel>>
    {
        private readonly IPermissionService _permissionService;

        public CreatePermissionHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(Commands.CreatePermissionCommand command, CancellationToken cancellationToken)
        {
            var permissionCreate = new PermissionReqRespModel { Id = command.Id, Name = command.Name};

            return await _permissionService.CreatePermission(permissionCreate);
        }
    }
}
