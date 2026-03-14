using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Roles.Commands;
using BDMS.Domain.Features.Roles.Models;
using BDMS.Domain.Features.Roles.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class RoleTests : IClassFixture<RoleApiFactory>
{
    private readonly HttpClient _client;

    public RoleTests(RoleApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllRoles_ReturnsOkWithRoleList()
    {
        var response = await _client.GetAsync("/api/Role/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<RolesReqRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("Admin", payload.Data![0].Name);
    }

    [Fact]
    public async Task CreateRole_ReturnsOkWithCreatedRole()
    {
        var request = new RolesReqRespModel { Id = 2, Name = "Staff" };
        var response = await _client.PostAsJsonAsync("/api/Role/create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolesReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("Staff", payload.Data!.Name);
    }

    [Fact]
    public async Task UpdateRole_ReturnsOkWithUpdatedRole()
    {
        var request = new RolesReqRespModel { Id = 1, Name = "SuperAdmin" };
        var response = await _client.PutAsJsonAsync("/api/Role/edit", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolesReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("SuperAdmin", payload.Data!.Name);
    }

    [Fact]
    public async Task DeleteRole_ReturnsOkWithDeleteMessage()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Role/delete")
        {
            Content = JsonContent.Create(new RolesReqRespModel { Id = 1 })
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<RolesReqRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }
}

public class RoleApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllRoles>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<RolesReqRespModel>>.Success([new RolesReqRespModel { Id = 1, Name = "Admin" }]));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateRoleCommand cmd, CancellationToken _) =>
                    Result<RolesReqRespModel>.Success(new RolesReqRespModel { Id = cmd.Id, Name = cmd.Name }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateRoleCommand cmd, CancellationToken _) =>
                    Result<RolesReqRespModel>.Success(new RolesReqRespModel { Id = cmd.Id, Name = cmd.Name }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RolesReqRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
