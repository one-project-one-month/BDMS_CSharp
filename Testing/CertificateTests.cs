using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BDMS.Domain.Features.Certificate.Commands;
using BDMS.Domain.Features.Certificate.Models;
using BDMS.Domain.Features.Certificate.Queries;
using BDMS.Shared;
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

public class CertificateTests : IClassFixture<CertificateApiFactory>
{
    private readonly HttpClient _client;

    public CertificateTests(CertificateApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GenerateCertificate_ReturnsOkWithGeneratedCertificate()
    {
        var request = new CertificateReqModel
        {
            DonorId = 10,
            CertificateTitle = "Achievement",
            CertificateDescription = "Donated 10 times"
        };

        var response = await _client.PostAsJsonAsync("/api/Certificate/Generate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<CertificateRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(10, payload.Data!.UserId);
        Assert.Equal("Achievement", payload.Data.CertificateTitle);
    }

    [Fact]
    public async Task GetCertificateById_ReturnsOkWithCertificate()
    {
        var response = await _client.GetAsync("/api/Certificate/7");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<CertificateRespModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(7, payload.Data!.Id);
    }

    [Fact]
    public async Task GetCertificatesByDonorId_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/Certificate/donor/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<CertificateRespModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
        Assert.Equal(10, payload.Data![0].UserId);
    }
}

public class CertificateApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IMediator));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, CertificateTestAuthHandler>("Test", _ => { });

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GenerateCertificateCommand cmd, CancellationToken _) =>
                    Result<CertificateRespModel>.Success(new CertificateRespModel
                    {
                        Id = 1,
                        UserId = cmd.Certificate.DonorId,
                        CertificateTitle = cmd.Certificate.CertificateTitle,
                        CertificateDescription = cmd.Certificate.CertificateDescription,
                        CertificateData = "BASE64",
                        CreatedAt = new DateOnly(2025, 1, 1)
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<GetCertificateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCertificateByIdQuery query, CancellationToken _) =>
                    Result<CertificateRespModel>.Success(new CertificateRespModel
                    {
                        Id = query.Id,
                        UserId = 10,
                        CertificateTitle = "Achievement",
                        CertificateDescription = "Donated 10 times",
                        CertificateData = "BASE64",
                        CreatedAt = new DateOnly(2025, 1, 1)
                    }));

            mediator
                .Setup(m => m.Send(It.IsAny<GetCertificatesByDonorIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCertificatesByDonorIdQuery query, CancellationToken _) =>
                    Result<List<CertificateRespModel>>.Success(
                    [
                        new CertificateRespModel
                        {
                            Id = 1,
                            UserId = query.DonorId,
                            CertificateTitle = "Achievement",
                            CertificateDescription = "Donated 10 times",
                            CertificateData = "BASE64",
                            CreatedAt = new DateOnly(2025, 1, 1)
                        }
                    ]));

            services.AddSingleton(mediator.Object);
        });
    }
}

public class CertificateTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CertificateTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
