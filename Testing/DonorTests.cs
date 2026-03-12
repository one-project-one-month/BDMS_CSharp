using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Donor.Commands;
using BDMS.Domain.Features.Donor.Models;
using BDMS.Domain.Features.Donor.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class DonorTests : IClassFixture<DonorApiFactory>
{
    private readonly HttpClient _client;

    public DonorTests(DonorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllDonorsList_ReturnsOkWithEmptyData()
    {
        var response = await _client.GetAsync("/api/Donor/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<List<DonorRespModel>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Empty(payload.Data!);
    }

    [Fact]
    public async Task GetDonorById_ReturnsOkWithDonorData()
    {
        var response = await _client.GetAsync("/api/Donor/Edit?donorId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonorRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("9/NRC111111", payload.Data!.NicNo);
    }

    [Fact]
    public async Task CreateDonor_ReturnsOkWithCreatedDonor()
    {
        var request = BuildDonorRequestModel();

        var response = await _client.PostAsJsonAsync("/api/Donor/Create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonorRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("A+", payload.Data!.BloodGroup);
    }

    [Fact]
    public async Task UpdateDonor_ReturnsOkWithUpdatedDonor()
    {
        var request = BuildDonorRequestModel();
        request.Id = 1;
        request.BloodGroup = "O+";

        var response = await _client.PutAsJsonAsync("/api/Donor/Update", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonorRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("O+", payload.Data!.BloodGroup);
    }

    [Fact]
    public async Task DeleteDonor_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/Donor/Delete?donorId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Result<DonorRespModel>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }

    private static DonorReqModel BuildDonorRequestModel() => new()
    {
        UserId = 2,
        NicNo = "9/NRC111111",
        DateOfBirth = new DateOnly(1995, 1, 12),
        Gender = "Male",
        BloodGroup = "A+",
        LastDonationDate = new DateOnly(2025, 1, 1),
        Remarks = "Healthy donor",
        EmergencyContact = "Jane",
        EmergencyPhone = "0991234567",
        Address = "Yangon",
        IsActive = true
    };
}

public class DonorApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllDonorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<DonorRespModel>>.Success([]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetAllDonorByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<DonorRespModel>.Success(new DonorRespModel
                {
                    Id = 1,
                    UserId = 2,
                    NicNo = "9/NRC111111",
                    DateOfBirth = new DateOnly(1995, 1, 12),
                    Gender = "Male",
                    BloodGroup = "A+",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateDonorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateDonorCommand command, CancellationToken _) =>
                {
                    return Result<DonorRespModel>.Success(new DonorRespModel
                    {
                        Id = 1,
                        UserId = command.UserId,
                        NicNo = command.NicNo,
                        DateOfBirth = command.DateOfBirth,
                        Gender = command.Gender,
                        BloodGroup = command.BloodGroup,
                        LastDonationDate = command.LastDonationDate,
                        Remarks = command.Remarks,
                        EmergencyContact = command.EmergencyContact,
                        EmergencyPhone = command.EmergencyPhone,
                        Address = command.Address,
                        IsActive = command.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateDonorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateDonorCommand command, CancellationToken _) =>
                {
                    return Result<DonorRespModel>.Success(new DonorRespModel
                    {
                        Id = command.Id,
                        UserId = command.UserId,
                        NicNo = command.NicNo,
                        DateOfBirth = command.DateOfBirth,
                        Gender = command.Gender,
                        BloodGroup = command.BloodGroup,
                        LastDonationDate = command.LastDonationDate,
                        Remarks = command.Remarks,
                        EmergencyContact = command.EmergencyContact,
                        EmergencyPhone = command.EmergencyPhone,
                        Address = command.Address,
                        IsActive = command.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                });

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteDonorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<DonorRespModel>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
