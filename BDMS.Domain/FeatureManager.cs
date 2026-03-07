using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Auth;
using BDMS.Domain.Features.User;
using BDMS.Shared;
using BDMS.Shared.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace BDMS.Domain;

public static class FeatureManager
{
    private static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<IAuthService,AuthService>();
        builder.Services.AddTransient<TokenService>();
    }
    
    public static void AddDomain(this WebApplicationBuilder builder)
    {
        // Configure DbContext with retry-on-failure
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

        }, ServiceLifetime.Transient, ServiceLifetime.Transient);

        // Register MediatR - scan the current assembly for handlers
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        builder.AddServices();

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
        builder.Services.AddSingleton(jwtSettings);          
        builder.Services
               .AddAuthentication(options =>
               {
                   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
               })
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = jwtSettings.Issuer,
                       ValidAudience = jwtSettings.Audience,
                       IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(jwtSettings.Key)),
                       ClockSkew = TimeSpan.Zero
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           var token = context.Request.Cookies[jwtSettings.AdminCookieName];
                           if (string.IsNullOrEmpty(token))
                           {
                               token = context.Request.Cookies[jwtSettings.ClientCookieName];
                           }
                           if (!string.IsNullOrEmpty(token))
                           {
                               context.Token = token;
                           }
                           return Task.CompletedTask;
                       }
                   };
               });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
            .AddPolicy("StaffOnly", policy => policy.RequireRole("staff"))
            .AddPolicy("DonorOnly", policy => policy.RequireRole("donor"))
            .AddPolicy("ClientOnly", policy => policy.RequireRole("user"));

        builder.Services.AddHttpContextAccessor();


    }
}
