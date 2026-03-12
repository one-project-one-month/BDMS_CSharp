using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.BloodRequest.Queries;
using BDMS.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BloodRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public BloodRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("List")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllBloodRequestsQuery(), ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> GetById([FromQuery] int bloodRequestId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBloodRequestByIdQuery { Id = bloodRequestId }, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] BloodRequestReqModel model, CancellationToken ct)
    {
        var command = new CreateBloodRequestCommand
        {
            UserId = model.UserId,
            HospitalId = model.HospitalId,
            PatientName = model.PatientName,
            BloodGroup = model.BloodGroup,
            UnitsRequired = model.UnitsRequired,
            ContactPhone = model.ContactPhone,
            Urgency = model.Urgency,
            RequiredDate = model.RequiredDate,
            Reason = model.Reason
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] BloodRequestReqModel model, CancellationToken ct)
    {
        var command = new UpdateBloodRequestCommand
        {
            Id = model.Id,
            UserId = model.UserId,
            HospitalId = model.HospitalId,
            PatientName = model.PatientName,
            BloodGroup = model.BloodGroup,
            UnitsRequired = model.UnitsRequired,
            ContactPhone = model.ContactPhone,
            Urgency = model.Urgency,
            RequiredDate = model.RequiredDate,
            Reason = model.Reason
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPatch("{id}/Status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateBloodRequestStatusReqModel model, CancellationToken ct)
    {
        if (!Enum.TryParse<EnumBloodRequestStatus>(model.Status, true, out var status) || status == EnumBloodRequestStatus.None)
            return BadRequest("Invalid status. Allowed values: pending, cancelled, approved, rejected, fulfilled.");

        var command = new UpdateBloodRequestStatusCommand
        {
            Id = id,
            Status = status,
            DonorId = model.DonorId
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromQuery] int bloodRequestId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteBloodRequestCommand { Id = bloodRequestId }, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
