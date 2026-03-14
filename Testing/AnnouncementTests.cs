using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Features.Announcement;
using BDMS.Domain.Features.Announcement.Models;
using BDMS.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Testing;

public class AnnouncementTests : IClassFixture<AnnouncementApiFactory>
{
    private readonly HttpClient _client;

    public AnnouncementTests(AnnouncementApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAnnouncements_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/Announcement/List");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<AnnouncementListItemResModel>>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Single(payload.Data!);
        Assert.Equal("Blood Drive", payload.Data![0].Title);
    }

    [Fact]
    public async Task GetAnnouncementById_ReturnsOkWithItem()
    {
        var response = await _client.GetAsync("/api/Announcement/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<GetAnnouncementByIdResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.Id);
    }

    [Fact]
    public async Task CreateAnnouncement_ReturnsOkWithCreatedItem()
    {
        var request = new CreateAnnouncementReqModel
        {
            Title = "New Event",
            Content = "Campaign",
            IsActive = true,
            ExpiredAt = new DateOnly(2026, 1, 10)
        };

        var response = await _client.PostAsJsonAsync("/api/Announcement", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<CreateAnnouncementResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal("New Event", payload.Data!.Title);
    }

    [Fact]
    public async Task UpdateAnnouncement_ReturnsOkWithUpdatedItem()
    {
        var request = new UpdateAnnouncementReqModel
        {
            Title = "Updated Event",
            Content = "Updated Campaign",
            IsActive = false,
            ExpiredAt = new DateOnly(2026, 2, 10)
        };

        var response = await _client.PutAsJsonAsync("/api/Announcement/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<UpdateAnnouncementResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.Id);
        Assert.Equal("Updated Event", payload.Data.Title);
    }

    [Fact]
    public async Task DeleteAnnouncement_ReturnsOkWithDeleteMessage()
    {
        var response = await _client.DeleteAsync("/api/Announcement/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<DeleteAnnouncementResModel>>();

        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data!.Id);
    }
}

public class AnnouncementApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IAnnouncementService));

            var service = new Mock<IAnnouncementService>();
            service
                .Setup(s => s.GetAnnouncements(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AnnouncementListItemResModel>>.Success(
                [
                    new() { Id = 1, Title = "Blood Drive", IsActive = true, CreatedAt = new DateOnly(2025, 1, 1) }
                ]));

            service
                .Setup(s => s.GetAnnouncementById(It.IsAny<GetAnnouncementByIdReqModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetAnnouncementByIdReqModel req, CancellationToken _) =>
                    Result<GetAnnouncementByIdResModel>.Success(new GetAnnouncementByIdResModel
                    {
                        Id = req.Id,
                        Title = "Blood Drive",
                        Content = "Details",
                        IsActive = true,
                        ExpiredAt = new DateOnly(2025, 12, 1),
                        CreatedAt = new DateOnly(2025, 1, 1),
                        UpdatedAt = new DateOnly(2025, 1, 2)
                    }));

            service
                .Setup(s => s.CreateAnnouncement(It.IsAny<CreateAnnouncementReqModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateAnnouncementReqModel req, CancellationToken _) =>
                    Result<CreateAnnouncementResModel>.Success(new CreateAnnouncementResModel
                    {
                        Id = 2,
                        Title = req.Title,
                        Content = req.Content,
                        IsActive = req.IsActive,
                        ExpiredAt = req.ExpiredAt,
                        CreatedAt = new DateOnly(2025, 1, 3)
                    }));

            service
                .Setup(s => s.UpdateAnnouncement(It.IsAny<UpdateAnnouncementReqModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UpdateAnnouncementReqModel req, CancellationToken _) =>
                    Result<UpdateAnnouncementResModel>.Success(new UpdateAnnouncementResModel
                    {
                        Id = req.Id,
                        Title = req.Title,
                        Content = req.Content,
                        IsActive = req.IsActive,
                        ExpiredAt = req.ExpiredAt,
                        UpdatedAt = new DateOnly(2025, 1, 4)
                    }));

            service
                .Setup(s => s.DeleteAnnouncement(It.IsAny<DeleteAnnouncementReqModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeleteAnnouncementReqModel req, CancellationToken _) =>
                    Result<DeleteAnnouncementResModel>.Success(new DeleteAnnouncementResModel
                    {
                        Id = req.Id,
                        Message = "Deleted"
                    }));

            services.AddSingleton(service.Object);
        });
    }
}
