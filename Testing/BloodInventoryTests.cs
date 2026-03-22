using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.BloodInventory.Commands;
using BDMS.Domain.Features.BloodInventory.Models;
using BDMS.Domain.Features.BloodInventory.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class BloodInventoryTests : IClassFixture<BloodInventoryApiFactory>
{
    private readonly HttpClient _client;

    public BloodInventoryTests(BloodInventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // TEST 1: GET /api/BloodInventory/list
    [Fact]
    public async Task GetAllInventory_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/BloodInventory/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<List<BloodInventoryResModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
    }

    // TEST 2: GET /api/BloodInventory/{id}
    [Fact]
    public async Task GetInventoryById_ReturnsOkWithCorrectRecord()
    {
        var response = await _client.GetAsync("/api/BloodInventory/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<BloodInventoryResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.Id);
        Assert.Equal("A+", payload.Data.BloodGroup);
        Assert.Equal("available", payload.Data.Status);
    }

    // TEST 3: GET /api/BloodInventory/hospital/{hospitalId}
    [Fact]
    public async Task GetInventoryByHospital_ReturnsOkWithFilteredData()
    {
        var response = await _client.GetAsync("/api/BloodInventory/hospital/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<List<BloodInventoryResModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.All(payload.Data!, item => Assert.Equal(1, item.HospitalId));
    }

    // TEST 4: GET /api/BloodInventory/available-stock
    [Fact]
    public async Task GetAvailableStock_ReturnsOkWithAggregatedData()
    {
        var response = await _client.GetAsync(
            "/api/BloodInventory/available-stock?hospitalId=1&bloodGroup=A%2B");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<List<AvailableStockResModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
        Assert.Equal(3, payload.Data![0].TotalUnits);
        Assert.Equal(2, payload.Data[0].AvailableCount);
    }

    // TEST 5: POST /api/BloodInventory/add/{donationId}
    [Fact]
    public async Task AddToInventory_ReturnsOkWithAvailableStatus()
    {
        var response = await _client.PostAsync(
            "/api/BloodInventory/add/10", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<BloodInventoryResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(10, payload.Data!.DonationId);
        Assert.Equal("available", payload.Data.Status);
    }

    // TEST 6: PATCH /api/BloodInventory/use
    [Fact]
    public async Task UseFromInventory_ReturnsOkWithUsedStatus()
    {
        var command = new UseFromInventoryCommand
        {
            BloodInventoryId = 1,
            RequestId = 5
        };

        var response = await _client.PatchAsJsonAsync(
            "/api/BloodInventory/use", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<AvailableStockResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.HospitalId);
    }

    // TEST 7: POST /api/BloodInventory/stock-take
    [Fact]
    public async Task RunStockTake_ReturnsOkWithExpiredCount()
    {
        var response = await _client.PostAsync(
            "/api/BloodInventory/stock-take?hospitalId=1", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<int>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(2, payload.Data);
    }

    // TEST 8: DELETE /api/BloodInventory/{id}
    [Fact]
    public async Task DeleteInventory_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/BloodInventory/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content
            .ReadFromJsonAsync<Result<string>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Data);
    }
}

// FACTORY: Mocks IMediator for all BloodInventory endpoints
public class BloodInventoryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();

            // ── GetAllBloodInventoryQuery ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<GetAllBloodInventoryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<BloodInventoryResModel>>.Success([
                    BuildSampleInventory()
                ]));

            // ── GetBloodInventoryByIdQuery ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<GetBloodInventoryByIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetBloodInventoryByIdQuery query, CancellationToken _) =>
                {
                    var item = BuildSampleInventory();
                    item.Id = query.Id;
                    return Result<BloodInventoryResModel>.Success(item);
                });

            // ── GetInventoryByHospitalQuery ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<GetInventoryByHospitalQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetInventoryByHospitalQuery query, CancellationToken _) =>
                {
                    var item = BuildSampleInventory();
                    item.HospitalId = query.HospitalId;
                    return Result<List<BloodInventoryResModel>>.Success([item]);
                });

            // ── GetAvailableStockQuery ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<GetAvailableStockQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetAvailableStockQuery query, CancellationToken _) =>
                    Result<List<AvailableStockResModel>>.Success([
                        new AvailableStockResModel
                        {
                            HospitalId = query.HospitalId ?? 1,
                            BloodGroup = query.BloodGroup ?? "A+",
                            TotalUnits = 3,
                            AvailableCount = 2
                        }
                    ]));

            // ── AddtoInventoryCommand ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<AddtoInventoryCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AddtoInventoryCommand command, CancellationToken _) =>
                {
                    var item = BuildSampleInventory();
                    item.DonationId = command.DonationId;
                    return Result<BloodInventoryResModel>.Success(item,
                        "Blood added to inventory successfully.");
                });

            // ── UseFromInventoryCommand ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<UseFromInventoryCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((UseFromInventoryCommand command, CancellationToken _) =>
                    Result<AvailableStockResModel>.Success(new AvailableStockResModel
                    {
                        HospitalId = 1,
                        BloodGroup = "A+",
                        TotalUnits = 2,
                        AvailableCount = 1
                    }, "Blood unit marked as used."));

            // ── RunStockTakeCommand ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<RunStockTakeCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(2, "2 unit(s) marked as expired."));

            // ── DeleteBloodInventoryCommand ──
            mediator
                .Setup(m => m.Send(
                    It.IsAny<DeleteBloodInventoryCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success("Deleting Successful."));

            services.AddSingleton(mediator.Object);
        });
    }

    private static BloodInventoryResModel BuildSampleInventory() => new()
    {
        Id = 1,
        DonationId = 10,
        HospitalId = 1,
        BloodGroup = "A+",
        Units = 1,
        CollectedAt = new DateOnly(2026, 3, 1),
        ExpiredAt = new DateOnly(2026, 4, 12),
        Status = "available",
        RequestId = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}