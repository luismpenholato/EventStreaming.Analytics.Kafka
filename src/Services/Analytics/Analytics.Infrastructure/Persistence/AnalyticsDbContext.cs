using Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<ProductMetric> ProductMetrics => Set<ProductMetric>();
    public DbSet<EventTypeMetric> EventTypeMetrics => Set<EventTypeMetric>();
    public DbSet<PeriodMetric> PeriodMetrics => Set<PeriodMetric>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<GlobalMetric> GlobalMetrics => Set<GlobalMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMetric>(e =>
        {
            e.ToTable("product_metrics");
            e.HasKey(x => x.ProductId);
        });

        modelBuilder.Entity<EventTypeMetric>(e =>
        {
            e.ToTable("event_type_metrics");
            e.HasKey(x => x.EventType);
        });

        modelBuilder.Entity<PeriodMetric>(e =>
        {
            e.ToTable("period_metrics");
            e.HasKey(x => new { x.PeriodStart, x.EventType });
        });

        modelBuilder.Entity<ProcessedEvent>(e =>
        {
            e.ToTable("processed_events");
            e.HasKey(x => x.EventId);
            e.HasIndex(x => x.ProcessedAt);
        });

        modelBuilder.Entity<GlobalMetric>(e =>
        {
            e.ToTable("global_metrics");
            e.HasKey(x => x.Id);
        });
    }
}
