using BDMS.Api.Filters;
using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CertificateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Generate")]
        [Permission("certificates.create")]
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

        [HttpGet("list")]
        [Permission("certificates.view")]
        public async Task<IActionResult> GetAllCertificates()
        {
            var query = new GetAllCertificatesQuery();
            var results = await _mediator.Send(query);
            if (!results.IsSuccess)
            {
                return BadRequest(results.Message);
            }
            return Ok(results);
        }

        [HttpGet("{id}")]
        [Permission("certificates.view")]
        public async Task<IActionResult> GetCertificateById([FromRoute] int id, CancellationToken ct)
        {
            var query = new GetCertificateByIdQuery() { Id = id };
            var result = await _mediator.Send(query, ct);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("donor/{donorId}")]
        [Permission("certificates.view")]
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
