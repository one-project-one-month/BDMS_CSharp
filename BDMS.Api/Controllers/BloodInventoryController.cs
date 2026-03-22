using BDMS.Domain.Features.BloodInventory.Commands;
using BDMS.Domain.Features.BloodInventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BloodInventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public BloodInventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllBloodInventoryQuery(), ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBloodInventoryByIdQuery { Id = id }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("hospital/{hospitalId}")]
    public async Task<IActionResult> GetByHospital([FromRoute] int hospitalId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetInventoryByHospitalQuery { HospitalId = hospitalId }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("available-stock")]
    public async Task<IActionResult> GetAvailableStock(
        [FromQuery] int? hospitalId, [FromQuery] string? bloodGroup, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAvailableStockQuery { HospitalId = hospitalId, BloodGroup = bloodGroup }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("add/{donationId}")]
    public async Task<IActionResult> AddToInventory([FromRoute] int donationId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddtoInventoryCommand { DonationId = donationId }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPatch("use")]
    public async Task<IActionResult> UseFromInventory(
        [FromBody] UseFromInventoryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("stock-take")]
    public async Task<IActionResult> RunStockTake(
        [FromQuery] int? hospitalId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RunStockTakeCommand { HospitalId = hospitalId }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeleteBloodInventoryCommand { Id = id }, ct);
        if (!result.IsSuccess) return BadRequest(result.Message);
        return Ok(result);
    }
}