using System.Text.Json.Serialization;
using HomeChef.Api.Common;
using HomeChef.Api.Middleware;
using HomeChef.Application;
using HomeChef.Infrastructure;
using HomeChef.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
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

app.Run();

public partial class Program;