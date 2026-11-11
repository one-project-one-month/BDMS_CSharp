using System.Security.Claims;
using BDMS.Api.Filters;
using BDMS.Database.AppDbContextModels;
using BDMS.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Testing;

public class PermissionFilterTests
{
    private static (ActionExecutingContext context, bool[] nextCalled) CreateActionContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), null!);
        var nextCalled = new[] { false };
        return (context, nextCalled);
    }

    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task OnActionExecutionAsync_Unauthenticated_ReturnsUnauthorized()
    {
        var db = CreateInMemoryDbContext();
        var filter = new PermissionFilter(new[] { "announcements.view" }, db);
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // IsAuthenticated = false
        var (context, nextCalled) = CreateActionContext(principal);

        await filter.OnActionExecutionAsync(context, () => { nextCalled[0] = true; return Task.FromResult<ActionExecutedContext>(null!); });

        Assert.False(nextCalled[0]);
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_AdminRole_BypassesAndCallsNext()
    {
        var db = CreateInMemoryDbContext();
        var filter = new PermissionFilter(new[] { "announcements.create" }, db);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "admin")
        }, "TestAuth");
        var (context, nextCalled) = CreateActionContext(new ClaimsPrincipal(identity));

        await filter.OnActionExecutionAsync(context, () => { nextCalled[0] = true; return Task.FromResult<ActionExecutedContext>(null!); });

        Assert.True(nextCalled[0]);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserWithMatchingClaim_CallsNext()
    {
        var db = CreateInMemoryDbContext();
        var filter = new PermissionFilter(new[] { "announcements.view" }, db);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "user"),
            new Claim("Permission", "announcements.view")
        }, "TestAuth");
        var (context, nextCalled) = CreateActionContext(new ClaimsPrincipal(identity));

        await filter.OnActionExecutionAsync(context, () => { nextCalled[0] = true; return Task.FromResult<ActionExecutedContext>(null!); });

        Assert.True(nextCalled[0]);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserWithMatchingDbPermission_CallsNext()
    {
        var db = CreateInMemoryDbContext();
        var perm = new Permission { Id = 1, Name = "announcements.update" };
        var role = new Role { Id = 1, Name = "editor" };
        var user = new User 
        { 
            Id = 5, 
            UserName = "editor_user", 
            Email = "editor@example.com", 
            Password = "hashedpassword", 
            RoleId = 1, 
            Role = role 
        };
        var rolePerm = new RolePermission { RoleId = 1, PermissionId = 1, Role = role, Permission = perm };

        db.Permissions.Add(perm);
        db.Roles.Add(role);
        db.Users.Add(user);
        db.RolePermissions.Add(rolePerm);
        await db.SaveChangesAsync();

        var filter = new PermissionFilter(new[] { "announcements.update" }, db);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "editor"),
            new Claim(ClaimTypes.NameIdentifier, EncryptionHelper.Encrypt("5"))
        }, "TestAuth");
        var (context, nextCalled) = CreateActionContext(new ClaimsPrincipal(identity));

        await filter.OnActionExecutionAsync(context, () => { nextCalled[0] = true; return Task.FromResult<ActionExecutedContext>(null!); });

        Assert.True(nextCalled[0]);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserMissingPermission_ReturnsForbid()
    {
        var db = CreateInMemoryDbContext();
        var filter = new PermissionFilter(new[] { "announcements.delete" }, db);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "user"),
            new Claim("Permission", "announcements.view")
        }, "TestAuth");
        var (context, nextCalled) = CreateActionContext(new ClaimsPrincipal(identity));

        await filter.OnActionExecutionAsync(context, () => { nextCalled[0] = true; return Task.FromResult<ActionExecutedContext>(null!); });

        Assert.False(nextCalled[0]);
        Assert.IsType<ForbidResult>(context.Result);
    }
}
