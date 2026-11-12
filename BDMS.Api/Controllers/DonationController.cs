using BDMS.Api.Filters;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donation.Queries;
using BDMS.Domain.Features.Donations.Commands;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Domain.Features.Donations.Queries;
using BDMS.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DonationController : ControllerBase
{
    private readonly IMediator _mediator;

    public DonationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("List")]
    [Permission("donations.view")]
    public async Task<IActionResult> GetAllDonation()
    {
        var query = new GetAllDonationQuery();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpGet("{HospitalId}/{DonationDate}")]
    [Permission("donations.view")]
    public async Task<IActionResult> GetDonationByDateAndHospital(int HospitalId, DateOnly DonationDate)
    {
        var query = new GetDonationByDateAndHospitalQuery()
        {
            HospitalId = HospitalId,
            DonationDate = DonationDate
        };

        var result = await _mediator.Send(query);
        if(!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);

    }

    [HttpPost("Create")]
    [Permission("donations.create")]
    public async Task<IActionResult> CreateDonation([FromBody] DonationCreateReqModel reqModel)
    {
        var command = new CreateDonationCommand()
        {
            DonorId = reqModel.DonorId,
            HospitalId = reqModel.HospitalId,
            BloodRequestId = reqModel.BloodRequestId,
            CreatedBy = reqModel.CreatedBy,
            BloodGroup = reqModel.BloodGroup,
            UnitsDonated = reqModel.UnitsDonated,
            DonationDate = reqModel.DonationDate,
            Remarks = reqModel.Remarks,
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpGet("Edit")]
    [Permission("donations.view")]
    public async Task<IActionResult> GetDonationById(int DonationId)
    {
        var query = new GetDonationByIdQuery()
        {
            Id = DonationId
        };

        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpPut("Update")]
    [Permission("donations.update")]

    public async Task<IActionResult> UpdateDonation(DonationUpdateReqModel reqModel)
    {
        var command = new UpdateDonationCommand()
        {
            Id = reqModel.Id,
            DonorId = reqModel.DonorId,
            HospitalId = reqModel.HospitalId,
            BloodRequestId = reqModel.BloodRequestId,
            DonationCode = reqModel.DonationCode,
            BloodGroup = reqModel.BloodGroup,
            UnitsDonated = reqModel.UnitsDonated,
            DonationDate = reqModel.DonationDate,
            Status = reqModel.Status,
            ApprovedBy = reqModel.ApprovedBy,
            ApprovedAt = reqModel.ApprovedAt,
            Remarks = reqModel.Remarks
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpPatch("UpdateStatus")]
    [Permission("donations.approve")]
    public async Task<IActionResult> UpdateStatus(UpdateDonationStatusReqModel reqModel)
    {
        if (!Enum.TryParse<EnumDonationStatus>(reqModel.Status, true, out var status) || status == EnumDonationStatus.None)
            return BadRequest("Invalid status. Allowed values: pending, cancelled, approved, rejected, completed, screening.");

        var command = new UpdateDonationStatusCommand()
        {
            Id = reqModel.Id,
            Status = status
        };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        { 
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpDelete("Delete")]
    [Permission("donations.delete")]

    public async Task<IActionResult> DeleteDonation(int DonationId)
    {
        var command = new DeleteDonationCommand()
        {
            Id = DonationId
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }
}
