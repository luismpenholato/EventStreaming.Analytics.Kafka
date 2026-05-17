using Analytics.Application;
using Analytics.Consumer.Workers;
using Analytics.Infrastructure;
using BuildingBlocks.Kafka;
using BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Serilog;

SerilogExtensions.ConfigureSerilog("Analytics.Consumer");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();

builder.Services
    .AddAnalyticsApplication()
    .AddAnalyticsInfrastructure(builder.Configuration)
    .AddKafkaConsumer(builder.Configuration);

builder.Services.AddHostedService<AnalyticsKafkaConsumer>();

var host = builder.Build();

await host.Services.MigrateDatabaseAsync();
await host.RunAsync();
