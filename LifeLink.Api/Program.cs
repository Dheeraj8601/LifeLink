using LifeLink.Api.Hubs;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Application.Services;
using LifeLink.Infrastructure.Data;
using LifeLink.Infrastructure.Repositories;
using LifeLink.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using LifeLink.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. Register API services
// --------------------------------------------------

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register SignalR
builder.Services.AddSignalR();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --------------------------------------------------
// 2. Configure Database
// --------------------------------------------------

var connectionString = builder.Configuration
    .GetConnectionString("LifeLinkConnection")
    ?? throw new InvalidOperationException(
        "LifeLink database connection string is not configured.");

builder.Services.AddDbContext<LifeLinkDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.UseNetTopologySuite()));

// --------------------------------------------------
// 3. Configure JWT Settings
// --------------------------------------------------

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
    jwtSettings.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException(
        "JWT configuration contains invalid values.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

// --------------------------------------------------
// 4. Configure Authentication
// --------------------------------------------------

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role
        };

        // Read JWT from HttpOnly cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token =
                    context.Request.Cookies["LifeLinkAuth"];

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DonorOnly", policy =>
        policy.RequireRole("Donor"));

    options.AddPolicy("RequesterOnly", policy =>
        policy.RequireRole("Requester"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// --------------------------------------------------
// 5. Register Repositories
// --------------------------------------------------

builder.Services.AddScoped<IDonorRepository, DonorRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IBloodRequestRepository, BloodRequestRepository>();
builder.Services.AddScoped<IRequestResponseRepository, RequestResponseRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IRequesterRepository, RequesterRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// --------------------------------------------------
// 6. Register Application Services
// --------------------------------------------------

builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
builder.Services.AddScoped<IRequestResponseService, RequestResponseService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IRequesterService, RequesterService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// --------------------------------------------------
// 7. Register Security Services
// --------------------------------------------------

builder.Services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// --------------------------------------------------
// 8. Build Application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// 9. Configure HTTP Pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// 10. Map Endpoints
// --------------------------------------------------

app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();