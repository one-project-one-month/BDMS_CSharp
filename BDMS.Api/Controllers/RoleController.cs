using BDMS.Domain.Features.Roles.Commands;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Domain.Features.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IMediator mediator;

        public RoleController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllRoles()
        {
            var query = new GetAllRoles();
            var results = await mediator.Send(query);
            if (!results.IsSuccess)
            {
                return BadRequest(results.Message);
            }
            return Ok(results);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(RolesReqRespModel model)
        {
            var command = new CreateRoleCommand { Id = model.Id, Name = model.Name };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> UpdateRole(RolesReqRespModel model)
        {
            var command = new UpdateRoleCommand { Id = model.Id, Name = model.Name };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteRole(RolesReqRespModel model)
        {
            var command = new DeleteRoleCommand { Id = model.Id };
            var result = await mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
