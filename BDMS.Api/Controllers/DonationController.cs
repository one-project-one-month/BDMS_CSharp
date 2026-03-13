using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donation.Queries;
using BDMS.Domain.Features.Donations.Commands;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Domain.Features.Donations.Queries;
using BDMS.Domain.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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


    [HttpPost("Create")]
    public async Task<IActionResult> CreateDonation(DonationCreateReqModel reqModel)
    {
        var command = new CreateDonationCommand()
        {
            DonorId = reqModel.DonorId,
            HospitalId = reqModel.HospitalId,
            BloodRequestId = reqModel.BloodRequestId,
            CreatedBy = reqModel.CreatedBy,
            DonationCode = reqModel.DonationCode,
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
    public async Task<IActionResult> GetDonationById(int DonationId)
    {
        var query = new GetDonationByIdQuery()
        {
            Id = DonationId
        };

        var result = await _mediator.Send(query);
        if(!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpPut("Update")]
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

    [HttpDelete("Delete")]
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
