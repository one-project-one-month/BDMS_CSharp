using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly JwtSettings _jwtSettings;
        public AuthController(IMediator mediator, JwtSettings jwtSettings)
        {
            _mediator = mediator;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginReqModel model)
        {
            var command = new AdminLoginCommand { Email = model.Email, Password = model.Password };

            var result = await _mediator.Send(command);

            if (result.IsError || result.Data == null)
                return Unauthorized(result);

            Response.Cookies.Append(
                _jwtSettings.AdminCookieName,
                result.Data.Token,
                BuildCookieOptions(result.Data.ExpireToken));

            return Excute(result);

        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(
                _jwtSettings.AdminCookieName,
                BuildCookieOptions(DateTime.Now)
            );

            return Ok(Result<string>.Success("Logout Successfully"));
        }

        private static CookieOptions BuildCookieOptions(DateTime expires) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = "/"
        };
    }
}
