using BDMS.Domain.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IMediator mediator;

        public DonationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetAllDonation()
        {
            var query = new GetAllUserQuery();
            var result = await mediator.Send(query);
            if(!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
