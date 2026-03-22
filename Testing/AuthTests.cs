using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class AuthTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthTests(AuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsOkAndSetsCookie()
    {
        var request = new AdminLoginReqModel { Email = "admin@bdms.com", Password = "password" };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie");

        var payload = await response.Content.ReadFromJsonAsync<Result<LoginResultInternal>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("admin@bdms.com", payload.Data!.UserInfo.Email);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/Auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Logout Successfully", payload.Data);
    }
}

public class AuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));
            services.RemoveAll(typeof(JwtSettings));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<AdminLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AdminLoginCommand command, CancellationToken _) =>
                    Result<LoginResultInternal>.Success(new LoginResultInternal
                    {
                        UserInfo = new LoginResModel
                        {
                            UserId = 1,
                            UserName = "admin",
                            Email = command.Email,
                            RoleName = "Admin",
                            Permissions = ["CanManageAll"]
                        },
                        Token = "fake-admin-token",
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
