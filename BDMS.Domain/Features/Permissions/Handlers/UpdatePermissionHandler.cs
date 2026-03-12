using BDMS.Domain.Features.Permissions.Commands;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class UpdatePermissionHandler : IRequestHandler<Commands.UpdatePermissionCommand, Result<PermissionReqRespModel>>
    {
        private readonly IPermissionService _permissionService;
        public UpdatePermissionHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            var update_model = new PermissionReqRespModel{Id = request.Id, Name = request.Name};

            return await _permissionService.UpdatePermission(update_model);
        }
    }
}
