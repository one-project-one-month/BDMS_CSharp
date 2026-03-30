using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Hospital.Commands;
using BDMS.Domain.Features.Hospital.Models;
using BDMS.Domain.Features.Hospital.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class HospitalTests : IClassFixture<HospitalApiFactory>
{
    private readonly HttpClient _client;

    public HospitalTests(HospitalApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllHospitals_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/Hospital/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<HospitalRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
    }

    [Fact]
    public async Task GetHospitalById_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/Hospital/get?id=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<HospitalRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.Id);
    }

    [Fact]
    public async Task UpdateHospital_ReturnsOkWithUpdatedData()
    {
        var request = new UpdateHospitalCommand
        {
            Id = 1,
            Name = "Central Hospital Updated",
            Address = "456 Updated Road",
            Phone = "555-3333",
            Email = "updated@hospital.com",
            IsVerified = true
        };

        var response = await _client.PutAsJsonAsync("/api/Hospital/update", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<HospitalRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("Central Hospital Updated", payload.Data!.Name);
        Assert.True(payload.Data.IsVerified);
    }

    [Fact]
    public async Task DeleteHospital_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/Hospital/delete?id=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<HospitalRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }
}

public class HospitalApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            services.AddTestAuthenticationAndAuthorization();
            var mediator = new Mock<IMediator>();

            mediator
                .Setup(m => m.Send(It.IsAny<GetAllHospitalsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<HospitalRespModel>>.Success([
                    new HospitalRespModel
                    {
                        Id = 1,
                        Name = "Central Hospital",
                        Address = "123 Main Street",
                        Phone = "555-1111",
                        Email = "central@hospital.com",
                        IsActive = true,
                        IsVerified = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetHospitalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetHospitalByIdQuery query, CancellationToken _) =>
                    Result<HospitalRespModel>.Success(new HospitalRespModel
                    {
                        Id = query.Id,
                        Name = "Central Hospital",
                        Address = "123 Main Street",
                        Phone = "555-1111",
                        Email = "central@hospital.com",
                        IsActive = true,
                        IsVerified = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateHospitalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateHospitalCommand command, CancellationToken _) =>
                    Result<HospitalRespModel>.Success(new HospitalRespModel
                    {
                        Id = command.Id,
                        Name = command.Name,
                        Address = command.Address,
                        Phone = command.Phone,
                        Email = command.Email,
                        IsActive = true,
                        IsVerified = command.IsVerified,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteHospitalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<HospitalRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
