using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Donation.Models;
using BDMS.Domain.Features.Donation.Queries;
using BDMS.Domain.Features.Donations.Commands;
using BDMS.Domain.Features.Donations.Models;
using BDMS.Domain.Features.Donations.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class DonationTests : IClassFixture<DonationApiFactory>
{
    private readonly HttpClient _client;

    public DonationTests(DonationApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllDonationList_ReturnsOkWithEmptyData()
    {
        var response = await _client.GetAsync("/api/Donation/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<List<DonationRespModel>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Empty(payload.Data!);
    }

    [Fact]
    public async Task GetDonationById_ReturnsOkWithDonationData()
    {
        var response = await _client.GetAsync("/api/Donation/Edit?donationId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonationRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("DN-001", payload.Data!.DonationCode);
    }

    [Fact]
    public async Task CreateDonation_ReturnsOkWithCreatedDonation()
    {
        var request = BuildCreateRequestModel();

        var response = await _client.PostAsJsonAsync("/api/Donation/Create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonationRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("A+", payload.Data!.BloodGroup);
    }

    [Fact]
    public async Task UpdateDonation_ReturnsOkWithUpdatedDonation()
    {
        var request = BuildUpdateRequestModel();
        request.BloodGroup = "O+";

        var response = await _client.PutAsJsonAsync("/api/Donation/Update", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonationRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("O+", payload.Data!.BloodGroup);
    }

    [Fact]
    public async Task DeleteDonation_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/Donation/Delete?donationId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonationRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }

    private static DonationCreateReqModel BuildCreateRequestModel() => new()
    {
        DonorId = 1,
        HospitalId = 2,
        BloodRequestId = 3,
        CreatedBy = 4,
        DonationCode = "DN-001",
        BloodGroup = "A+",
        UnitsDonated = 1,
        DonationDate = new DateOnly(2025, 1, 12),
        Remarks = "Initial donation"
    };

    private static DonationUpdateReqModel BuildUpdateRequestModel() => new()
    {
        Id = 1,
        DonorId = 1,
        HospitalId = 2,
        BloodRequestId = 3,
        DonationCode = "DN-001",
        BloodGroup = "A+",
        UnitsDonated = 1,
        DonationDate = new DateOnly(2025, 1, 12),
        Status = "pending",
        ApprovedBy = null,
        ApprovedAt = null,
        Remarks = "Updated donation"
    };
}

public class DonationApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllDonationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<DonationRespModel>>.Success([]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetDonationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<DonationRespModel>.Success(new DonationRespModel
                {
                    Id = 1,
                    DonorId = 1,
                    HospitalId = 2,
                    BloodRequestId = 3,
                    CreatedBy = 4,
                    DonationCode = "DN-001",
                    BloodGroup = "A+",
                    UnitsDonated = 1,
                    DonationDate = new DateOnly(2025, 1, 12),
                    Status = "pending",
                    Remarks = "Initial donation",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateDonationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateDonationCommand command, CancellationToken _) =>
                {
                    return Result<DonationRespModel>.Success(new DonationRespModel
                    {
                        Id = 1,
                        DonorId = command.DonorId,
                        HospitalId = command.HospitalId,
                        BloodRequestId = command.BloodRequestId,
                        CreatedBy = command.CreatedBy,
                        DonationCode = command.DonationCode,
                        BloodGroup = command.BloodGroup,
                        UnitsDonated = command.UnitsDonated,
                        DonationDate = command.DonationDate,
                        Status = "pending",
                        ApprovedBy = command.ApprovedBy,
                        ApprovedAt = command.ApprovedAt,
                        Remarks = command.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateDonationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateDonationCommand command, CancellationToken _) =>
                {
                    return Result<DonationRespModel>.Success(new DonationRespModel
                    {
                        Id = command.Id,
                        DonorId = command.DonorId,
                        HospitalId = command.HospitalId,
                        BloodRequestId = command.BloodRequestId,
                        CreatedBy = command.CreatedBy,
                        DonationCode = command.DonationCode,
                        BloodGroup = command.BloodGroup,
                        UnitsDonated = command.UnitsDonated,
                        DonationDate = command.DonationDate,
                        Status = command.Status,
                        ApprovedBy = command.ApprovedBy,
                        ApprovedAt = command.ApprovedAt,
                        Remarks = command.Remarks,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteDonationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<DonationRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
