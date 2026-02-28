using BDMS.Api.Controllers;
using BDMS.Application.Users;
using BDMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDMS.Api.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task CreateUserAsync_ReturnsCreatedAtAction_WithCreatedUserId()
    {
        var userId = Guid.NewGuid();
        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        var controller = new UsersController(userService.Object);

        var response = await controller.CreateUserAsync(
            new CreateUserRequest { Username = "tester", Email = "tester@example.com" },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(UsersController.GetAllAsync), created.ActionName);
        Assert.Equal(userId, created.Value);

        userService.Verify(x => x.CreateUserAsync(
            It.Is<User>(u => u.Username == "tester" && u.Email == "tester@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
