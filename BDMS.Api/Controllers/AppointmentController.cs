using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Domain.Features.Appointment.Queries;
using BDMS.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy="AdminStaff")]
    public class AppointmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AppointmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetAllAppointmentList(CancellationToken ct)
        {
            var query = new GetAllAppointmentQuery();
            var result = await _mediator.Send(query, ct);
            
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById([FromRoute] int id, CancellationToken ct)
        {
            var query = new GetAppointmentByIdQuery() { Id = id };
            var result = await _mediator.Send(query, ct);
            
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpPost("donation/{donationId}")]
        public async Task<IActionResult> CreateDonationAppointment([FromRoute] int donationId, [FromBody] AppointmentReqModel request, CancellationToken ct)
        {
            var command = new CreateDonationAppointmentCommand()
            {
                DonationId = donationId,
                Remarks = request.Remarks
            };
            
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpPost("blood-request/{bloodRequestId}")]
        public async Task<IActionResult> CreateBloodRequestAppointment([FromRoute]int bloodRequestId, [FromBody] AppointmentReqModel request, CancellationToken ct)
        {
            var command = new CreateBloodRequestAppointmentCommand()
            {
                BloodRequestId = bloodRequestId,
                Remarks = request.Remarks
            };
            
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus([FromRoute] int id, [FromBody] UpdateAppointmentStatusReqModel request, CancellationToken ct)
        {
            if (!Enum.TryParse<EnumAppointmentStatus>(request.Status, true, out var status) ||
                status == EnumAppointmentStatus.None)
            {
                return BadRequest("Invalid status. Allowed values: scheduled, confirmed, cancelled, completed.");
            }

            var command = new UpdateAppointmentStatusCommand()
            {
                Id = id,
                Status = status
            };
            
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteAppointment([FromRoute]int id, CancellationToken ct)
        {
            var command = new CompleteAppointmentCommand() { Id = id };
            var result = await _mediator.Send(command, ct);
            
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] int id, CancellationToken ct)
        {
            var command = new DeleteAppointmentCommand() { Id = id };
            var result = await _mediator.Send(command, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
