using BDMS.Domain;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File($"log/{Assembly.GetEntryAssembly()?.GetName().Name}.log", rollingInterval: RollingInterval.Hour)
    .CreateLogger();

EnsureValidAppSettingsJson();

try
{
    Log.Information("Starting Web Application");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173") // change frontend url
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // if you send cookies or auth headers
        });
    });

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Donation System", Version = "v1" });
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });

    builder.AddDomain();

    var app = builder.Build();

    app.UseCors("AllowFrontend");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void EnsureValidAppSettingsJson()
{
    var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(appSettingsPath))
    {
        return;
    }

    try
    {
        using var stream = File.OpenRead(appSettingsPath);
        using var _ = JsonDocument.Parse(stream);
    }
    catch (JsonException ex)
    {
        var backupPath = $"{appSettingsPath}.invalid.{DateTime.UtcNow:yyyyMMddHHmmss}";
        File.Move(appSettingsPath, backupPath, overwrite: true);
        Log.Warning(ex,
            "Invalid JSON in appsettings.json. Renamed file to {BackupPath}. " +
            "Provide a valid appsettings.json or environment variables for configuration.",
            backupPath);
    }
}

public partial class Program { }
