using System.Net;
using System.Net.Http.Json;
using BDMS.Api.Controllers;
using BDMS.Domain.Features.RolePermission.Command;
using BDMS.Domain.Features.RolePermission.Model;
using BDMS.Domain.Features.RolePermission.Query;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class RolePermissionTests : IClassFixture<RolePermissionApiFactory>
{
    private readonly HttpClient _client;

    public RolePermissionTests(RolePermissionApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllRolePermissions_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/RolePermission/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<RolePermissionReqRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
        Assert.Equal(1, payload.Data![0].RoleId);
    }

    [Fact]
    public async Task CreateRolePermission_ReturnsOkWithCreatedPair()
    {
        var request = new RolePermissionReqRespModel { RoleId = 2, PermissionId = 3 };
        var response = await _client.PostAsJsonAsync("/api/RolePermission/create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolePermissionReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(2, payload.Data!.RoleId);
        Assert.Equal(3, payload.Data.PermissionId);
    }

    [Fact]
    public async Task UpdateRolePermission_ReturnsOkWithUpdatedPair()
    {
        var request = new UpdateRolePermissionRequest
        {
            OldModel = new RolePermissionReqRespModel { RoleId = 1, PermissionId = 1 },
            NewModel = new RolePermissionReqRespModel { RoleId = 1, PermissionId = 5 }
        };

        var response = await _client.PutAsJsonAsync("/api/RolePermission/edit", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolePermissionReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.RoleId);
        Assert.Equal(5, payload.Data.PermissionId);
    }

    [Fact]
    public async Task DeleteRolePermission_ReturnsOkWithDeleteMessage()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/RolePermission/delete")
        {
            Content = JsonContent.Create(new RolePermissionReqRespModel { RoleId = 1, PermissionId = 1 })
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolePermissionReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }
}

public class RolePermissionApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllRolePermission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<RolePermissionReqRespModel>>.Success([new RolePermissionReqRespModel { RoleId = 1, PermissionId = 1 }]));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateRolePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateRolePermissionCommand cmd, CancellationToken _) =>
                    Result<RolePermissionReqRespModel>.Success(new RolePermissionReqRespModel { RoleId = cmd.RoleId, PermissionId = cmd.PermissionId }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateRolePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateRolePermissionCommand cmd, CancellationToken _) =>
                    Result<RolePermissionReqRespModel>.Success(new RolePermissionReqRespModel { RoleId = cmd.newRoleId, PermissionId = cmd.newPermissionId }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteRolePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RolePermissionReqRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
