using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using HomeChef.Api.Common;
using HomeChef.Api.Middleware;
using HomeChef.Application;
using HomeChef.Application.Security;
using HomeChef.Domain.Constants;
using HomeChef.Infrastructure;
using HomeChef.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

        var response = new ApiErrorResponse(
            new ApiError(
                "VALIDATION_ERROR",
                errors.Count == 0
                    ? "One or more fields failed validation."
                    : $"One or more fields failed validation: {string.Join("; ", errors.Select(kv => $"{kv.Key}={string.Join(",", kv.Value)}"))}"));

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must be configured and at least 32 characters long (see 'Jwt:SigningKey').");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Accept the token from the auth cookie (browser) or the
                // Authorization header (API clients).
                var token = context.Request.Cookies[jwtOptions.CookieName];
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Headers.Authorization.ToString()
                        .Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
                }

                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireCustomer, policy => policy.RequireRole(Roles.Customer));
    options.AddPolicy(Policies.RequireChef, policy => policy.RequireRole(Roles.Chef));
    options.AddPolicy(Policies.RequireAdmin, policy => policy.RequireRole(Roles.Admin));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<HomeChefDbContext>("database", tags: ["ready"]);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Map("/api/{**path}", () => Results.Json(
    new ApiErrorResponse(new ApiError("NOT_FOUND", "The requested resource was not found.")),
    statusCode: StatusCodes.Status404NotFound));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
            }),
        };

        await context.Response.WriteAsJsonAsync(response);
    },
});

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();

public partial class Program;