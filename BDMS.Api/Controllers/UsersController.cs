using BDMS.Application.Users;
using BDMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BDMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var userId = await _userService.CreateUserAsync(new User
        {
            Username = request.Username,
            Email = request.Email
        }, cancellationToken);

        return CreatedAtAction(nameof(GetAllAsync), new { id = userId }, userId);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
