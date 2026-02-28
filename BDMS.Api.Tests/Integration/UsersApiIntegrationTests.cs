using System.Net;
using System.Net.Http.Json;
using BDMS.Domain.Entities;

namespace BDMS.Api.Tests.Integration;

public class UsersApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostUser_ThenGetUsers_ReturnsCreatedUser()
    {
        var payload = new
        {
            username = "integration-user",
            email = "integration-user@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdUserId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, createdUserId);

        var getResponse = await _client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var users = await getResponse.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.Contains(users!, user =>
            user.Id == createdUserId &&
            user.Username == payload.username &&
            user.Email == payload.email);
    }
}
