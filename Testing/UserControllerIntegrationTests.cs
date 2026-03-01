using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.User.Models;
using BDMS.Domain.Features.User.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Testing;

public class UserControllerIntegrationTests : IClassFixture<UserApiFactory>
{
    private readonly HttpClient _client;

    public UserControllerIntegrationTests(UserApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllUserList_ReturnsOkWithUserData()
    {
        var response = await _client.GetAsync("/api/User/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<List<UserRespModel>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Single(payload.Data);
        Assert.Equal("USR001", payload.Data[0].UserId);
    }
}

public class UserApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<UserRespModel>>.Success(new List<UserRespModel>
                {
                    new() { UserId = "USR001", Username = "test.user", PhoneNo = "0991234567" }
                }));

            services.AddSingleton(mediator.Object);
        });
    }
}
