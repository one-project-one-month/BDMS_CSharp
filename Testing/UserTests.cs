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
        var response = await _client.GetAsync("/api/User/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<List<UserRespModel>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
        Assert.Equal(1, payload.Data![0].UserId);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOkWithUpdatedUser()
    {
        var request = new UserReqModel
        {
            UserId = 1,
            Username = "updated.user",
            Email = "updated.user@example.com",
            UserRoleId = 2,
            UserHospitalId = 3
        };

        var response = await _client.PutAsJsonAsync("/api/User/update", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<UserRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.UserId);
        Assert.Equal("updated.user", payload.Data.Username);
        Assert.Equal("updated.user@example.com", payload.Data.Email);
    }

    [Fact]
    public async Task CreateUser_ReturnsOkWithCreatedUser()
    {
        var request = new UserReqModel
        {
            UserId = 5,
            Username = "new.user",
            Email = "new.user@example.com",
            UserRoleId = 1,
            UserHospitalId = 2
        };

        var response = await _client.PostAsJsonAsync("/api/User/create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<UserRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(5, payload.Data!.UserId);
        Assert.Equal("new.user", payload.Data.Username);
    }

    [Fact]
    public async Task DeleteUser_ReturnsOkWithDeleteMessage()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/User/delete")
        {
            Content = JsonContent.Create(new UserReqModel { UserId = 1, Username = "test.user" })
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<UserRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }

    [Fact]
    public async Task GetUserByParameter_ReturnsOkWithUserData()
    {
        var request = new UserReqModel
        {
            UserId = 5,
            Username = "new.user",
        };

        var response = await _client.PostAsJsonAsync("/api/User/userbyparameter", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<UserRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(5, payload.Data!.UserId);
        Assert.Equal("new.user", payload.Data.Username);
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
                    new() { UserId = 1, Username = "test.user", Email = "test.user@example.com", UserRoleId = 2, UserHospitalId = 3 }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateUserCommand command, CancellationToken _) =>
                    Result<UserRespModel>.Success(new UserRespModel
                    {
                        UserId = command.UserId,
                        Username = command.UserName,
                        Email = command.Email,
                        UserRoleId = command.UserRoleId,
                        UserHospitalId = command.hospital_id
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteUserByParameterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<UserRespModel>.DeleteSuccess());

            mediator
                .Setup(m => m.Send(It.IsAny<GetUserByParameterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUserByParameterCommand command, CancellationToken _) =>
                    Result<UserRespModel>.Success(new UserRespModel
                    {
                        UserId = command.UserId,
                        Username = command.UserName,
                        Email = "test.user@example.com",
                        UserRoleId = 2,
                        UserHospitalId = 3
                    }));

            services.AddSingleton(mediator.Object);
        });
    }
}
