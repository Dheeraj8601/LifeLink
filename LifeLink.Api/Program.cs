
using System.Security.Claims;
using System.Text;
using LifeLink.Api.BackgroundJobs;
using Hangfire;
using Hangfire.SqlServer;

using LifeLink.Api.Authorization;
using LifeLink.Api.Hubs;
using LifeLink.Api.Services;

using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Application.Services;

using LifeLink.Infrastructure.Data;
using LifeLink.Infrastructure.Email;
using LifeLink.Infrastructure.Repositories;
using LifeLink.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. Register API services
// --------------------------------------------------

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

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
// 3. Configure Hangfire Background Jobs
// --------------------------------------------------

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            connectionString,
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),

                SlidingInvisibilityTimeout =
                    TimeSpan.FromMinutes(5),

                QueuePollInterval = TimeSpan.Zero,

                UseRecommendedIsolationLevel = true,

                DisableGlobalLocks = true
            });
});

// Start Hangfire background worker.
builder.Services.AddHangfireServer();
builder.Services.AddScoped<BloodRequestDonorNotificationJob>();
builder.Services.AddScoped<IBloodRequestNotificationScheduler,HangfireBloodRequestNotificationScheduler>();
builder.Services.AddScoped<BloodRequestDonorNotificationJob>();

// --------------------------------------------------
// 4. Configure JWT Settings
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
// 5. Configure Email Settings
// --------------------------------------------------

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, MailKitEmailService>();

// --------------------------------------------------
// 6. Configure Authentication
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

        // Read JWT from HttpOnly cookie.
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

// --------------------------------------------------
// 7. Configure Authorization
// --------------------------------------------------

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
// 8. Register Repositories
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
// 9. Register Application Services
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
// 10. Register Security Services
// --------------------------------------------------

builder.Services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// --------------------------------------------------
// 11. Build Application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// 12. Configure HTTP Pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// 13. Hangfire Dashboard - Admin Only
// --------------------------------------------------

app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions
    {
        Authorization =
        [
            new HangfireDashboardAuthorizationFilter()
        ]
    });

// --------------------------------------------------
// 14. Map Endpoints
// --------------------------------------------------

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
