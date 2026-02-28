using BDMS.Application.Donors.Commands;
using BDMS.Application.Donors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DonorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateDonorCommand command, CancellationToken cancellationToken)
    {
        var donorId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = donorId }, donorId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var donor = await _mediator.Send(new GetDonorByIdQuery(id), cancellationToken);

        if (donor is null)
        {
            return NotFound();
        }

        return Ok(donor);
    }
}
