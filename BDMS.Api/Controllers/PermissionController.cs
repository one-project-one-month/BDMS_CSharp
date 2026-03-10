using BDMS.Domain.Features.Permissions.Commands;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Domain.Features.Permissions.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : Controller
    {
        private readonly IMediator mediator;

        public PermissionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var query = new GetAllPermissions();
            var results = await mediator.Send(query);
            if (!results.IsSuccess)
            {
                return BadRequest(results.Message);
            }
            return Ok(results);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreatePermission(PermissionReqRespModel model) 
        {
            var command = new CreatePermissionCommand { Id = model.Id, Name = model.Name };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("Edit")]
        public async Task<IActionResult> UpdatePermission(PermissionReqRespModel model)
        {
            var command = new UpdatePermissionCommand { Id = model.Id, Name = model.Name };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeletePermission(PermissionReqRespModel model)
        {
            var command = new DeletePermissionCommand { Id = model.Id };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
