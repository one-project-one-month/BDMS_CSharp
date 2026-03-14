using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminStaff")]
    public class CertificateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CertificateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateCertificate([FromBody] CertificateReqModel request, CancellationToken ct)
        {
            var command = new GenerateCertificateCommand()
            {
                Certificate = request
            };

            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificateById([FromRoute] int id, CancellationToken ct)
        {
            var query = new GetCertificateByIdQuery() { Id = id };
            var result = await _mediator.Send(query, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("donor/{donorId}")]
        public async Task<IActionResult> GetCertificatesByDonorId([FromRoute] int donorId, CancellationToken ct)
        {
            var query = new GetCertificatesByDonorIdQuery() { DonorId = donorId };
            var result = await _mediator.Send(query, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
