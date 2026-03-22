using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.UserAuth.Commands;
using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class UserAuthTests : IClassFixture<UserAuthApiFactory>
{
    private readonly HttpClient _client;

    public UserAuthTests(UserAuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsOkAndSetsCookie()
    {
        var request = new UserLoginReqModel { Email = "user@bdms.com", Password = "password" };

        var response = await _client.PostAsJsonAsync("/api/UserAuth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");

        var payload = await response.Content.ReadFromJsonAsync<Result<UserLoginResultInternal>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("user@bdms.com", payload.Data!.UserInfo.Email);
    }

    [Fact]
    public async Task Register_ReturnsOkAndSetsCookie()
    {
        var request = new UserRegisterReqModel
        {
            UserName = "test.user",
            Email = "user@bdms.com",
            Password = "password",
            ConfirmPassword = "password"
        };

        var response = await _client.PostAsJsonAsync("/api/UserAuth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");

        var payload = await response.Content.ReadFromJsonAsync<Result<UserLoginResultInternal>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("test.user", payload.Data!.UserInfo.UserName);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/UserAuth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Logout Successfully", payload.Data);
    }
}

public class UserAuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));
            services.RemoveAll(typeof(JwtSettings));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserLoginCommand command, CancellationToken _) =>
                    Result<UserLoginResultInternal>.Success(new UserLoginResultInternal
                    {
                        UserInfo = new UserAuthResModel
                        {
                            UserId = 2,
                            UserName = "test.user",
                            Email = command.Email,
                            RoleName = "User",
                            Permissions = ["CanCreateBloodRequest"]
                        },
                        Token = "fake-user-token",
                        ExpireToken = DateTime.UtcNow.AddHours(1)
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UserRegisterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserRegisterCommand command, CancellationToken _) =>
                    Result<UserLoginResultInternal>.Success(new UserLoginResultInternal
                    {
                        UserInfo = new UserAuthResModel
                        {
                            UserId = 2,
                            UserName = command.UserName,
                            Email = command.Email,
                            RoleName = "User",
                            Permissions = ["CanCreateBloodRequest"]
                        },
                        Token = "fake-register-token",
                        ExpireToken = DateTime.UtcNow.AddHours(1)
                    }));

            services.AddSingleton(mediator.Object);
            services.AddSingleton(new JwtSettings
            {
                Key = "test-key",
                Issuer = "test",
                Audience = "test",
                ExpireMinutes = 60,
                AdminCookieName = "BDMS_Admin_Token",
                ClientCookieName = "BDMS_Client_Token"
            });
        });
    }
}
