using BuildingBlocks.Observability;
using Serilog;
using Tracking.Application;
using Tracking.Infrastructure;

SerilogExtensions.ConfigureSerilog("Tracking.Api");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services
    .AddTrackingApplication()
    .AddTrackingInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("TrackingDb")!)
    .AddKafkaHealthCheck(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateTrackingDatabaseAsync();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        if (ex is FluentValidation.ValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
    });
});

app.UseCorrelationId();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("kafka") });

app.Run();

public partial class Program;
