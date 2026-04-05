using BDMS.Domain.Features.Hospital.Commands;
using BDMS.Domain.Features.Hospital.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HospitalController : ControllerBase
{
    private readonly IMediator _mediator;

    public HospitalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [Authorize(Policy = "AdminClient")]
    public async Task<IActionResult> GetAllHospitals()
    {
        var query = new GetAllHospitalsQuery();
        var result = await _mediator.Send(query);

        if(!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("get")]
    [Authorize(Policy = "AdminClient")]
    public async Task<IActionResult> GetHospitalById(int id)
    {
        var query = new GetHospitalByIdQuery()
        {
            Id = id
        };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPut("update")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateHospital([FromBody] UpdateHospitalCommand reqModel)
    {
        var result = await _mediator.Send(reqModel);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("delete")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteHospital(int id)
    {
        var command = new DeleteHospitalCommand()
        {
            Id = id
        };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
