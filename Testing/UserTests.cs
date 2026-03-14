using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.User.Commands;
using BDMS.Domain.Features.User.Models;
using BDMS.Domain.Features.User.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class UserTests : IClassFixture<UserApiFactory>
{
    private readonly HttpClient _client;

    public UserTests(UserApiFactory factory)
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

        if (payload.Data is not null)
        {
            Assert.Single(payload.Data);
            Assert.Equal(1, payload.Data[0].UserId);
        }
    }

    [Fact]
    public async Task UpdateUser_ReturnsOkWithUpdatedPhoneNumber()
    {
        var response = await _client.PutAsync("/api/User/Update?UserId=USR001&PhoneNo=09111222333", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<UserRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.UserId);
        Assert.Equal("test.user@example.com", payload.Data.Email);
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
                .ReturnsAsync(Result<List<UserRespModel>>.Success(
                [
                    new() { UserId = 1, Username = "test.user", Email = "test.user@example.com" }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateUserCommand command, CancellationToken _) =>
                    Result<UserRespModel>.Success(new UserRespModel
                    {
                        UserId = command.UserId,
                        Username = "test.user",
                        Email = "test.user@example.com"
                    }));

            services.AddSingleton(mediator.Object);
        });
    }
}
