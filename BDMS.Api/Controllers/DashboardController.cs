using BDMS.Domain.Features.Dashboard.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly IMediator _mediatR;

        public DashboardController(IMediator mediatR)
        {
            _mediatR = mediatR;
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetDashboardById([FromRoute] int userId, CancellationToken ct) 
        {
            var command = new GetDashboardByIdQuery { user_Id = userId};

            var result = await _mediatR.Send(command, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Content(result.Data ?? "{}", "application/json");
        }
    }
}
