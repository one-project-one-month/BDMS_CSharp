using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Testing;

public static class TestAuthExtensions
{
    public static IServiceCollection AddTestAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
            options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
        }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("StaffOnly", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("DonorOnly", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("DonarOnly", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("ClientOnly", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("AdminStaff", policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
