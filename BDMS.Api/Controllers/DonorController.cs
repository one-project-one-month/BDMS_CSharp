using BDMS.Api.Filters;
using BDMS.Domain.Features.Donor.Commands;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Donor.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpGet("list")]
    [Permission("donors.view")]

    public async Task<IActionResult> GetAllDonorsList()
    {
        var query = new GetAllDonorsQuery();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("create")]
    [Permission("donors.create")]
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
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    // Self-service: any authenticated user applies; record starts pending until admin/staff activates it
    [HttpPost("become")]
    [Authorize]
    public async Task<IActionResult> BecomeDonor(DonorReqModel reqModel)
    {
        var encryptedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(encryptedUserId))
            return Unauthorized();

        var userId = int.Parse(EncryptionHelper.Decrypt(encryptedUserId));
        var command = new BecomeDonorCommand
        {
            UserId = userId,
            NicNo = reqModel.NicNo,
            DateOfBirth = reqModel.DateOfBirth,
            Gender = reqModel.Gender,
            BloodGroup = reqModel.BloodGroup,
            LastDonationDate = reqModel.LastDonationDate,
            Remarks = reqModel.Remarks,
            EmergencyContact = reqModel.EmergencyContact,
            EmergencyPhone = reqModel.EmergencyPhone,
            Address = reqModel.Address
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Permission("donors.view")]

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

    [HttpPut("update")]
    [Permission("donors.update")]

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
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }


    [HttpPatch("{id}/status")]
    [Permission("donors.update")]
    public async Task<IActionResult> UpdateDonorStatus(int id, [FromBody] DonorStatusReqModel model)
    {
        var command = new DonorStatusCommand { DonorId = id, IsActive = model.IsActive };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("delete")]
    [Permission("donors.delete")]

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
