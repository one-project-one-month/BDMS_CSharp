using BDMS.Domain.Features.Auth.Models;
using BDMS.Domain.Features.Auth.Queries;
using BDMS.Domain.Features.UserAuth.Commands;
using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BDMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly JwtSettings _jwtSettings;

        public UserAuthController(IMediator mediator, JwtSettings jwtSettings)
        {
            _mediator = mediator;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel model)
        {
            var command = new UserLoginCommand { Email = model.Email, Password = model.Password };

            var result = await _mediator.Send(command);

            if (result.IsError || result.Data == null)
                return Unauthorized(result);

            Response.Cookies.Delete(_jwtSettings.AdminCookieName, BuildCookieOptions(DateTime.Now));

            Response.Cookies.Append(
                _jwtSettings.ClientCookieName,
                result.Data.Token,
                BuildCookieOptions(result.Data.ExpireToken));

            return Execute(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterReqModel model)
        {
            var command = new UserRegisterCommand
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            var result = await _mediator.Send(command);

            if (result.IsError || result.Data == null)
                return BadRequest(result);

            Response.Cookies.Append(
                _jwtSettings.ClientCookieName,
                result.Data.Token,
                BuildCookieOptions(result.Data.ExpireToken));

            return Execute(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(
                _jwtSettings.ClientCookieName,
                BuildCookieOptions(DateTime.Now)
            );

            return Ok(Result<string>.Success("Logout Successfully"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
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
