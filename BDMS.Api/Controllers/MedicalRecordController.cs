using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Domain.Features.MedicalRecord.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MedicalRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicalRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllMedicalRecordsQuery(), ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMedicalRecordByIdQuery { Id = id }, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MedicalRecordReqModel model, CancellationToken ct)
    {
        var command = new CreateMedicalRecordCommand
        {
            DonationId = model.DonationId,
            HospitalId = model.HospitalId,
            HemoglobinLevel = model.HemoglobinLevel,
            HivResult = model.HivResult,
            HepatitisBResult = model.HepatitisBResult,
            HepatitisCResult = model.HepatitisCResult,
            MalariaResult = model.MalariaResult,
            SyphilisResult = model.SyphilisResult,
            ScreeningStatus = model.ScreeningStatus,
            ScreeningNotes = model.ScreeningNotes,
            ScreenedBy = model.ScreenedBy,
            ScreeningAt = model.ScreeningAt
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] MedicalRecordReqModel model, CancellationToken ct)
    {
        var command = new UpdateMedicalRecordCommand
        {
            Id = model.Id,
            DonationId = model.DonationId,
            HospitalId = model.HospitalId,
            HemoglobinLevel = model.HemoglobinLevel,
            HivResult = model.HivResult,
            HepatitisBResult = model.HepatitisBResult,
            HepatitisCResult = model.HepatitisCResult,
            MalariaResult = model.MalariaResult,
            SyphilisResult = model.SyphilisResult,
            ScreeningStatus = model.ScreeningStatus,
            ScreeningNotes = model.ScreeningNotes,
            ScreenedBy = model.ScreenedBy,
            ScreeningAt = model.ScreeningAt
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteMedicalRecordCommand { Id = id }, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
