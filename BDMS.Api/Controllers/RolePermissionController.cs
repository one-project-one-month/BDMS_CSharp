using BDMS.Domain.Features.RolePermission.Command;
using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Domain.Features.RolePermission.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : Controller
    {
        private readonly IMediator mediator;

        public RolePermissionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllRolePermissions()
        {
            var query = new GetAllRolePermission();
            var results = await mediator.Send(query);
            if (!results.IsSuccess)
            {
                return BadRequest(results.Message);
            }
            return Ok(results);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRolePermission(RolePermissionReqRespModel model)
        {
            var command = new CreateRolePermissionCommand { RoleId = model.RoleId, PermissionId = model.PermissionId };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> UpdateRolePermission([FromBody] UpdateRolePermissionRequest model)
        {
            var command = new UpdateRolePermissionCommand 
            { oldRoleId = model.OldModel.RoleId, oldPermissionId = model.OldModel.PermissionId, newRoleId = model.NewModel.RoleId, newPermissionId = model.NewModel.PermissionId };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteRolePermission(RolePermissionReqRespModel model)
        {
            var command = new DeleteRolePermissionCommand { RoleId = model.RoleId, PermissionId = model.PermissionId };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

    }

    public class UpdateRolePermissionRequest
    {
        public RolePermissionReqRespModel OldModel { get; set; } = new RolePermissionReqRespModel();
        public RolePermissionReqRespModel NewModel { get; set; } = new RolePermissionReqRespModel();
    }
}
