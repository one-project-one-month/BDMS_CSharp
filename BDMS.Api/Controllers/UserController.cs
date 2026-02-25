using BDMS.Domain.Features.User.Commands;
using BDMS.Domain.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("List")]
        public async Task<IActionResult> GetAllUserList()
        {
            var query = new GetAllUserQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateUser(string UserId, string PhoneNo)
        {
            var command = new UpdateUserCommand
            {
                UserId = UserId,
                PhoneNo = PhoneNo
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
