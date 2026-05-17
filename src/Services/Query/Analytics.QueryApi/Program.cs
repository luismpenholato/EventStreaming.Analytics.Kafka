using Analytics.QueryInfrastructure;
using BuildingBlocks.Observability;
using Serilog;

SerilogExtensions.ConfigureSerilog("Analytics.QueryApi");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddQueryInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("AnalyticsDb")!)
    .AddKafkaHealthCheck(builder.Configuration);

var app = builder.Build();

app.UseCorrelationId();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new() { Predicate = _ => true });

app.Run();

public partial class Program;
