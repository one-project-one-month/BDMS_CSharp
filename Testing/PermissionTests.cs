using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Permissions.Commands;
using BDMS.Domain.Features.Permissions.Models;
using BDMS.Domain.Features.Permissions.Query;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class PermissionTests : IClassFixture<PermissionApiFactory>
{
    private readonly HttpClient _client;

    public PermissionTests(PermissionApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllPermissions_ReturnsOkWithPermissionData()
    {
        var response = await _client.GetAsync("/api/Permission/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<List<PermissionReqRespModel>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);

        if (payload.Data is not null)
        {
            Assert.Single(payload.Data);
            Assert.Equal("CanViewUsers", payload.Data[0].Name);
        }
    }

    [Fact]
    public async Task CreatePermission_ReturnsOkWithCreatedPermission()
    {
        var request = new PermissionReqRespModel { Id = 10, Name = "CanCreateDonor" };

        var response = await _client.PostAsJsonAsync("/api/Permission/Create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<PermissionReqRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("CanCreateDonor", payload.Data!.Name);
    }

    [Fact]
    public async Task UpdatePermission_ReturnsOkWithUpdatedPermission()
    {
        var request = new PermissionReqRespModel { Id = 10, Name = "CanEditDonor" };

        var response = await _client.PutAsJsonAsync("/api/Permission/Edit", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<PermissionReqRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("CanEditDonor", payload.Data!.Name);
    }

    [Fact]
    public async Task DeletePermission_ReturnsOkWithDeleteMessage()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Permission/Delete")
        {
            Content = JsonContent.Create(new PermissionReqRespModel { Id = 10 })
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<PermissionReqRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }
}

public class PermissionApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllPermissions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<PermissionReqRespModel>>.Success(
                [
                    new() { Id = 1, Name = "CanViewUsers" }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreatePermissionCommand command, CancellationToken _) =>
                {
                    return Result<PermissionReqRespModel>.Success(new PermissionReqRespModel
                    {
                        Id = command.Id,
                        Name = command.Name
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<UpdatePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdatePermissionCommand command, CancellationToken _) =>
                {
                    return Result<PermissionReqRespModel>.Success(new PermissionReqRespModel
                    {
                        Id = command.Id,
                        Name = command.Name
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<DeletePermissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PermissionReqRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
