using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.Permissions;
using BDMS.Domain.Features.Donor;
using BDMS.Domain.Features.Auth;
using BDMS.Domain.Features.User;
using BDMS.Domain.Features.UserAuth;
using BDMS.Domain.Features.Announcement;
using BDMS.Domain.Features.Appointment;
using BDMS.Domain.Features.BloodRequest;
using BDMS.Domain.Features.MedicalRecord;
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
using BDMS.Domain.Features.Roles;
using BDMS.Domain.Features.Certificate;

namespace BDMS.Domain;

public static class FeatureManager
{
    private static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserAuthService, UserAuthService>();
        builder.Services.AddScoped<IAppointmentService, AppointmentService>();
        builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddScoped<IDonorService, DonorService>();
        builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
        builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<RoleService>();
        builder.Services.AddScoped<ICertificateService, CertificateService>();
    }
    
    public static void AddDomain(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

        }, ServiceLifetime.Transient, ServiceLifetime.Transient);

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
            .AddPolicy("AdminStaff", policy => policy.RequireRole("admin", "staff"))
            .AddPolicy("StaffOnly", policy => policy.RequireRole("staff"))
            .AddPolicy("DonorOnly", policy => policy.RequireRole("donor"))
            .AddPolicy("ClientOnly", policy => policy.RequireRole("user"));

        builder.Services.AddHttpContextAccessor();


    }
}
