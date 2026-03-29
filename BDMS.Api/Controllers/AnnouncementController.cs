using BDMS.Domain.Features.Announcement.Commands;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Domain.Features.Announcement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnnouncementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("List")]
        //[Authorize(Policy = "AdminClientDonar")]
        public async Task<IActionResult> GetAnnouncements([FromQuery] string? category, CancellationToken ct)
        {
            var query = new GetAnnouncementsQuery { Category = category };
            var result = await _mediator.Send(query, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("{id}")]
        //[Authorize(Policy = "AdminClientDonar")]
        public async Task<IActionResult> GetAnnouncementById([FromRoute] int id, CancellationToken ct)
        {
            var query = new GetAnnouncementByIdQuery(id);
            var result = await _mediator.Send(query, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost]
        //[Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementCommand request, CancellationToken ct)
        {
            var result = await _mediator.Send(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateAnnouncement([FromRoute] int id, [FromBody] UpdateAnnouncementCommand request, CancellationToken ct)
        {
            request.Id = id;
            var result = await _mediator.Send(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteAnnouncement([FromRoute] int id, CancellationToken ct)
        {
            var command = new DeleteAnnouncementCommand(id);
            var result = await _mediator.Send(command, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
