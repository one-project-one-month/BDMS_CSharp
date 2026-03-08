using BDMS.Domain.Features.Donor.Commands;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Donor.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]

public class DonorController : ControllerBase
{
    private readonly IMediator _mediator;

    public DonorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("List")]
    public async Task<IActionResult> GetAllDonorsList()
    {
        var query = new GetAllDonorsQuery();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateDonor(DonorReqModel reqModel)
    {
        var command = new CreateDonorCommand
        {
            UserId = reqModel.UserId,
            NicNo = reqModel.NicNo,
            DateOfBirth = reqModel.DateOfBirth,
            Gender = reqModel.Gender,
            BloodGroup = reqModel.BloodGroup,
            LastDonationDate = reqModel.LastDonationDate,
            Remarks = reqModel.Remarks,
            EmergencyContact = reqModel.EmergencyContact,
            EmergencyPhone = reqModel.EmergencyPhone,
            Address = reqModel.Address,
            IsActive = reqModel.IsActive
        };

        var result = await _mediator.Send(command);
        if(!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> GetDonorById(int donorId)
    {
        var query = new GetAllDonorByIdQuery
        {
            Id = donorId
        };

        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateDonor(DonorReqModel reqModel)
    {
        var command = new UpdateDonorCommand()
        {
            Id = reqModel.Id,
            UserId = reqModel.UserId,
            NicNo = reqModel.NicNo,
            DateOfBirth = reqModel.DateOfBirth,
            Gender = reqModel.Gender,
            BloodGroup = reqModel.BloodGroup,
            LastDonationDate = reqModel.LastDonationDate,
            Remarks = reqModel.Remarks,
            EmergencyContact = reqModel.EmergencyContact,
            EmergencyPhone = reqModel.EmergencyPhone,
            Address = reqModel.Address,
            IsActive = reqModel.IsActive
        };

        var result = await _mediator.Send(command);
        if(!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteDonorAsync(int donorId)
    {
        var command = new DeleteDonorCommand
        {
            Id = donorId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest("Error deleting donor");

        return Ok(result);
    }
}
