using BDMS.Domain.Features.Announcement;
using BDMS.Domain.Features.Announcement.Models;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetAnnouncements(CancellationToken ct)
        {
            var result = await _announcementService.GetAnnouncements(ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncementById([FromRoute] int id, CancellationToken ct)
        {
            var request = new GetAnnouncementByIdReqModel { Id = id };
            var result = await _announcementService.GetAnnouncementById(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementReqModel request, CancellationToken ct)
        {
            var result = await _announcementService.CreateAnnouncement(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement([FromRoute] int id, [FromBody] UpdateAnnouncementReqModel request, CancellationToken ct)
        {
            request.Id = id;
            var result = await _announcementService.UpdateAnnouncement(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement([FromRoute] int id, CancellationToken ct)
        {
            var request = new DeleteAnnouncementReqModel { Id = id };
            var result = await _announcementService.DeleteAnnouncement(request, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
