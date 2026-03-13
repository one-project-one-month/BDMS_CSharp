using BDMS.Domain.Features.Permissions.Commands;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.Permissions.Handlers
{
    public class DeletePermissionHandler : IRequestHandler<Commands.DeletePermissionCommand, Result<PermissionReqRespModel>>
    {

        private readonly IPermissionService _permissionService;
        public DeletePermissionHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<Result<PermissionReqRespModel>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            return await _permissionService.DeletePermissionById(request.Id);
        }
    }
}
