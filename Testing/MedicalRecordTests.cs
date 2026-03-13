using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.MedicalRecord.Commands;
using BDMS.Domain.Features.MedicalRecord.Models;
using BDMS.Domain.Features.MedicalRecord.Queries;
using BDMS.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class MedicalRecordTests : IClassFixture<MedicalRecordApiFactory>
{
    private readonly HttpClient _client;

    public MedicalRecordTests(MedicalRecordApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllMedicalRecords_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/MedicalRecord/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<MedicalRecordRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
    }

    [Fact]
    public async Task CreateMedicalRecord_ReturnsOkWithData()
    {
        var response = await _client.PostAsJsonAsync("/api/MedicalRecord/create", BuildRequestModel());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<MedicalRecordRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("negative", payload.Data!.HivResult);
    }

    [Fact]
    public async Task GetMedicalRecordById_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/MedicalRecord/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<MedicalRecordRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.DonationId);
    }

    [Fact]
    public async Task UpdateMedicalRecord_ReturnsOkWithUpdatedData()
    {
        var request = BuildRequestModel();
        request.Id = 1;
        request.ScreeningStatus = "passed";

        var response = await _client.PutAsJsonAsync("/api/MedicalRecord/update", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<MedicalRecordRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("passed", payload.Data!.ScreeningStatus);
    }

    [Fact]
    public async Task DeleteMedicalRecord_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/MedicalRecord/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Message);
    }

    private static MedicalRecordReqModel BuildRequestModel() => new()
    {
        DonationId = 1,
        HospitalId = 1,
        HemoglobinLevel = 13.5m,
        HivResult = "negative",
        HepatitisBResult = "negative",
        HepatitisCResult = "negative",
        MalariaResult = "negative",
        SyphilisResult = "negative",
        ScreeningStatus = "pending",
        ScreeningNotes = "Initial screening",
        ScreenedBy = 100,
        ScreeningAt = DateTime.UtcNow
    };
}

public class MedicalRecordApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            var mediator = new Mock<IMediator>();

            mediator
                .Setup(m => m.Send(It.IsAny<GetAllMedicalRecordsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<MedicalRecordRespModel>>.Success([
                    new MedicalRecordRespModel
                    {
                        Id = 1,
                        DonationId = 1,
                        HospitalId = 1,
                        HemoglobinLevel = 13.2m,
                        HivResult = "negative",
                        HepatitisBResult = "negative",
                        HepatitisCResult = "negative",
                        MalariaResult = "negative",
                        SyphilisResult = "negative",
                        ScreeningStatus = "pending",
                        ScreeningNotes = "Initial screening",
                        ScreenedBy = 100,
                        ScreeningAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetMedicalRecordByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<MedicalRecordRespModel>.Success(new MedicalRecordRespModel
                {
                    Id = 1,
                    DonationId = 1,
                    HospitalId = 1,
                    HemoglobinLevel = 13.2m,
                    HivResult = "negative",
                    HepatitisBResult = "negative",
                    HepatitisCResult = "negative",
                    MalariaResult = "negative",
                    SyphilisResult = "negative",
                    ScreeningStatus = "pending",
                    ScreeningNotes = "Initial screening",
                    ScreenedBy = 100,
                    ScreeningAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateMedicalRecordCommand command, CancellationToken _) =>
                    Result<MedicalRecordRespModel>.Success(new MedicalRecordRespModel
                    {
                        Id = 1,
                        DonationId = command.DonationId,
                        HospitalId = command.HospitalId,
                        HemoglobinLevel = command.HemoglobinLevel,
                        HivResult = command.HivResult,
                        HepatitisBResult = command.HepatitisBResult,
                        HepatitisCResult = command.HepatitisCResult,
                        MalariaResult = command.MalariaResult,
                        SyphilisResult = command.SyphilisResult,
                        ScreeningStatus = command.ScreeningStatus,
                        ScreeningNotes = command.ScreeningNotes,
                        ScreenedBy = command.ScreenedBy,
                        ScreeningAt = command.ScreeningAt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateMedicalRecordCommand command, CancellationToken _) =>
                    Result<MedicalRecordRespModel>.Success(new MedicalRecordRespModel
                    {
                        Id = command.Id,
                        DonationId = command.DonationId,
                        HospitalId = command.HospitalId,
                        HemoglobinLevel = command.HemoglobinLevel,
                        HivResult = command.HivResult,
                        HepatitisBResult = command.HepatitisBResult,
                        HepatitisCResult = command.HepatitisCResult,
                        MalariaResult = command.MalariaResult,
                        SyphilisResult = command.SyphilisResult,
                        ScreeningStatus = command.ScreeningStatus,
                        ScreeningNotes = command.ScreeningNotes,
                        ScreenedBy = command.ScreenedBy,
                        ScreeningAt = command.ScreeningAt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.DeleteSuccess());

            services.AddSingleton(mediator.Object);
        });
    }
}
