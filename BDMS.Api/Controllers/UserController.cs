using BDMS.Api.Filters;
using BDMS.Domain.Features.User.Commands;
using BDMS.Domain.Features.User.Models;
using BDMS.Domain.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("list")]
        [Permission("users.view")]
        public async Task<IActionResult> GetAllUserList()
        {
            var query = new GetAllUserQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("update")]
        [Permission("users.update")]
        public async Task<IActionResult> UpdateUser(UserReqModel model)
        {
            var command = new UpdateUserCommand
            {
                UserId = model.UserId,
                UserRoleId = model.UserRoleId,
                Email = model.Email,
                hospital_id = model.UserHospitalId ?? null,
                UserName = model.Username,
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("create")]
        [Permission("users.create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserReqModel model)
        {
            var command = new CreateUserCommand
            {
                UserRoleId = model.UserRoleId,
                Email = model.Email,
                Password = model.Password,
                hospital_id = model.UserHospitalId,
                UserName = model.Username,
            };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpDelete("delete/{userId}")]
        [Permission("users.delete")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            var command = new DeleteUserByParameterCommand { UserId = userId };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission("users.view")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var command = new GetUserByParameterCommand { UserId = id };
            var result = await _mediator.Send(command);
            if(!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        [Permission("users.update")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatusReqModel model)
        {
            var command = new UserStatusCommand { UserId = id, IsActive = model.IsActive };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            return Ok(result);
        }
    }
}
