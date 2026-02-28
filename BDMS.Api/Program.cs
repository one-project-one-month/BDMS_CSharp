using System.Reflection;
using BDMS.Application.Users;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        sqlOptions.EnableRetryOnFailure();
    }));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("BDMS.Application")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
