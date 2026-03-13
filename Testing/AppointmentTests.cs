using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BDMS.Domain.Features.Appointment.Commands;
using BDMS.Domain.Features.Appointment.Models;
using BDMS.Domain.Features.Appointment.Queries;
using BDMS.Shared;
using BDMS.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Testing;

public class AppointmentTests : IClassFixture<AppointmentApiFactory>
{
    private readonly HttpClient _client;

    public AppointmentTests(AppointmentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAppointments_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/Appointment/list");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<AppointmentRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/Appointment/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<AppointmentRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(EnumAppointmentStatus.Scheduled, payload.Data!.Status);
    }

    [Fact]
    public async Task CreateDonationAppointment_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/Appointment/donation/4", new AppointmentReqModel { Remarks = "Donor visit" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<AppointmentRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(4, payload.Data!.DonationId);
    }

    [Fact]
    public async Task CreateBloodRequestAppointment_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/Appointment/blood-request/7", new AppointmentReqModel { Remarks = "Urgent request" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<AppointmentRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(7, payload.Data!.BloodRequestId);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_ReturnsOkWithUpdatedStatus()
    {
        var response = await _client.PatchAsJsonAsync("/api/Appointment/1/status", new UpdateAppointmentStatusReqModel { Status = "confirmed" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<AppointmentRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(EnumAppointmentStatus.Confirmed, payload.Data!.Status);
    }

    [Fact]
    public async Task CompleteAppointment_ReturnsOkWithCompletedStatus()
    {
        var response = await _client.PostAsync("/api/Appointment/1/complete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<AppointmentRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal(EnumAppointmentStatus.Completed, payload.Data!.Status);
    }

    [Fact]
    public async Task DeleteAppointment_AfterCreate_ReturnsOkWithDeleteMessage()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Appointment/donation/4", new AppointmentReqModel { Remarks = "delete flow" });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var response = await _client.DeleteAsync("/api/Appointment/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.Equal("Deleting Successful.", payload.Data);
    }
}

public class AppointmentApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminStaff", policy => policy.RequireAuthenticatedUser());
            });

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GetAllAppointmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AppointmentRespModel>>.Success([
                    new AppointmentRespModel
                    {
                        UserId = 1,
                        HospitalId = 2,
                        DonationId = 4,
                        AppointmentDate = new DateOnly(2026, 1, 1),
                        AppointmentTime = new TimeOnly(10, 0),
                        Status = EnumAppointmentStatus.Scheduled,
                        Remarks = "Follow up"
                    }
                ]));

            mediator
                .Setup(m => m.Send(It.IsAny<GetAppointmentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AppointmentRespModel>.Success(new AppointmentRespModel
                {
                    UserId = 1,
                    HospitalId = 2,
                    DonationId = 4,
                    AppointmentDate = new DateOnly(2026, 1, 1),
                    AppointmentTime = new TimeOnly(10, 0),
                    Status = EnumAppointmentStatus.Scheduled,
                    Remarks = "Follow up"
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateDonationAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateDonationAppointmentCommand command, CancellationToken _) =>
                    Result<AppointmentRespModel>.Success(new AppointmentRespModel
                    {
                        UserId = 1,
                        HospitalId = 2,
                        DonationId = command.DonationId,
                        AppointmentDate = new DateOnly(2026, 1, 1),
                        AppointmentTime = new TimeOnly(10, 0),
                        Status = EnumAppointmentStatus.Scheduled,
                        Remarks = command.Remarks
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<CreateBloodRequestAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateBloodRequestAppointmentCommand command, CancellationToken _) =>
                    Result<AppointmentRespModel>.Success(new AppointmentRespModel
                    {
                        UserId = 1,
                        HospitalId = 2,
                        BloodRequestId = command.BloodRequestId,
                        AppointmentDate = new DateOnly(2026, 1, 1),
                        AppointmentTime = new TimeOnly(10, 0),
                        Status = EnumAppointmentStatus.Scheduled,
                        Remarks = command.Remarks
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<UpdateAppointmentStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateAppointmentStatusCommand command, CancellationToken _) =>
                    Result<AppointmentRespModel>.Success(new AppointmentRespModel
                    {
                        UserId = 1,
                        HospitalId = 2,
                        DonationId = 4,
                        AppointmentDate = new DateOnly(2026, 1, 1),
                        AppointmentTime = new TimeOnly(10, 0),
                        Status = command.Status,
                        Remarks = "updated"
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<CompleteAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<AppointmentRespModel>.Success(new AppointmentRespModel
                {
                    UserId = 1,
                    HospitalId = 2,
                    DonationId = 4,
                    AppointmentDate = new DateOnly(2026, 1, 1),
                    AppointmentTime = new TimeOnly(10, 0),
                    Status = EnumAppointmentStatus.Completed,
                    Remarks = "done"
                }));

            mediator
                .Setup(m => m.Send(It.IsAny<DeleteAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success("Deleting Successful."));

            services.AddSingleton(mediator.Object);
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "test-admin") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
