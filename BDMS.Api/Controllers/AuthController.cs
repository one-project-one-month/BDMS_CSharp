using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Domain.Features.Auth.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

            Response.Cookies.Delete(_jwtSettings.ClientCookieName, BuildCookieOptions(DateTime.Now));

            Response.Cookies.Append(
                _jwtSettings.AdminCookieName,
                result.Data.Token,
                BuildCookieOptions(result.Data.ExpireToken));

            return Execute(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(
                _jwtSettings.AdminCookieName,
                BuildCookieOptions(DateTime.Now)
            );

            return Ok(Result<string>.Success("Logout Successfully"));
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role) || !new[] { "admin", "staff" }.Contains(role.ToLower()))
            {
                Response.Cookies.Delete(_jwtSettings.AdminCookieName, BuildCookieOptions(DateTime.Now));
                return Unauthorized(Result<string>.ValidationError("Access denied. Admin or staff role required."));
            }

            var encryptedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(encryptedUserId))
                return Unauthorized();

            var userId = int.Parse(EncryptionHelper.Decrypt(encryptedUserId));

            var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
            return Execute(result);

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
