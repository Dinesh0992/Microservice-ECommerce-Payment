using Microsoft.EntityFrameworkCore;
using MassTransit;
using Order.Service.Models;

namespace Order.Service.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Orders> Orders => Set<Orders>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Orders>()
        .Property(o => o.Amount)
        .HasColumnType("decimal(18,2)");

        // This creates the internal tables MassTransit needs to manage the Outbox
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}