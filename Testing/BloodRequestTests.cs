using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.BloodRequest.Commands;
using BDMS.Domain.Features.BloodRequest.Models;
using BDMS.Domain.Features.BloodRequest.Queries;
using BDMS.Shared;
using BDMS.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class BloodRequestTests : IClassFixture<BloodRequestApiFactory>
{
    private readonly HttpClient _client;

    public BloodRequestTests(BloodRequestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllBloodRequests_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/BloodRequest/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<BloodRequestRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
    }

    [Fact]
    public async Task CreateBloodRequest_ReturnsOkWithPendingStatus()
    {
        var response = await _client.PostAsJsonAsync("/api/BloodRequest/Create", BuildRequestModel());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<BloodRequestRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(EnumBloodRequestStatus.Pending, payload.Data!.Status);
    }

    [Fact]
    public async Task UpdateBloodRequestStatus_WithDonor_ReturnsOkWithApprovedStatus()
    {
        var response = await _client.PatchAsJsonAsync("/api/BloodRequest/1/Status", new UpdateBloodRequestStatusReqModel
        {
            Status = "approved",
            DonorId = 7
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<BloodRequestRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(EnumBloodRequestStatus.Approved, payload.Data!.Status);
        Assert.Equal(100, payload.Data.ApprovedBy);
    }

    private static BloodRequestReqModel BuildRequestModel() => new()
    {
        UserId = 2,
        HospitalId = 1,
        PatientName = "John Doe",
        BloodGroup = "A+",
        UnitsRequired = 2,
        ContactPhone = "099999999",
        Urgency = "high",
        RequiredDate = new DateOnly(2026, 1, 2),
        Reason = "Emergency surgery"
    };
}

public class BloodRequestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();

            mediator
                .Setup(m => m.Send(It.IsAny<GetAllBloodRequestsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<BloodRequestRespModel>>.Success([
                    new BloodRequestRespModel
                    {
                        Id = 1,
                        UserId = 2,
                        HospitalId = 1,
                        PatientName = "John Doe",
                        BloodGroup = EnumBloodGroup.APositive,
                        UnitsRequired = 2,
                        Urgency = "high",
                        Status = EnumBloodRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetBloodRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<BloodRequestRespModel>.Success(new BloodRequestRespModel
                {
                    Id = 1,
                    UserId = 2,
                    HospitalId = 1,
                    PatientName = "John Doe",
                    BloodGroup = EnumBloodGroup.APositive,
                    UnitsRequired = 2,
                    Urgency = "high",
                    Status = EnumBloodRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateBloodRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateBloodRequestCommand command, CancellationToken _) =>
                    Result<BloodRequestRespModel>.Success(new BloodRequestRespModel
                    {
                        Id = 1,
                        UserId = command.UserId,
                        HospitalId = command.HospitalId,
                        PatientName = command.PatientName,
                        BloodGroup = EnumBloodGroup.APositive,
                        UnitsRequired = command.UnitsRequired,
                        ContactPhone = command.ContactPhone,
                        Urgency = command.Urgency,
                        RequiredDate = command.RequiredDate,
                        Reason = command.Reason,
                        Status = EnumBloodRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateBloodRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateBloodRequestCommand command, CancellationToken _) =>
                    Result<BloodRequestRespModel>.Success(new BloodRequestRespModel
                    {
                        Id = command.Id,
                        UserId = command.UserId,
                        HospitalId = command.HospitalId,
                        PatientName = command.PatientName,
                        BloodGroup = EnumBloodGroup.APositive,
                        UnitsRequired = command.UnitsRequired,
                        ContactPhone = command.ContactPhone,
                        Urgency = command.Urgency,
                        RequiredDate = command.RequiredDate,
                        Reason = command.Reason,
                        Status = EnumBloodRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateBloodRequestStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateBloodRequestStatusCommand command, CancellationToken _) =>
                    Result<BloodRequestRespModel>.Success(new BloodRequestRespModel
                    {
                        Id = command.Id,
                        UserId = 2,
                        HospitalId = 1,
                        PatientName = "John Doe",
                        BloodGroup = EnumBloodGroup.APositive,
                        UnitsRequired = 2,
                        Urgency = "high",
                        Status = command.Status,
                        ApprovedBy = 100,
                        ApprovedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteBloodRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success(null, "Deleting Successful."));

            services.AddSingleton(mediator.Object);
        });
    }
}
