using Order.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Order.Service.Workers;

public class PaymentTimeoutWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentTimeoutWorker> _logger;

    public PaymentTimeoutWorker(IServiceProvider serviceProvider, ILogger<PaymentTimeoutWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // This loop runs as long as the microservice is alive
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Janitor Service: Scanning for timed-out orders...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                // Logic: Find orders in 'PaymentInitiated' or 'Pending' older than 30 minutes
                var timeoutTime = DateTime.UtcNow.AddMinutes(-30);

                var stuckOrders = await context.Orders
                    .Where(o => (o.Status == "PaymentInitiated" || o.Status == "Pending") 
                                 && o.CreatedAt < timeoutTime)
                    .ToListAsync();

                foreach (var order in stuckOrders)
                {
                    order.Status = "TimedOut";
                    _logger.LogWarning($"Cleaning up Order {order.Id}: Marked as TimedOut.");
                }

                if (stuckOrders.Any())
                {
                    await context.SaveChangesAsync();
                }
            }

            // Wait 5 minutes before checking again
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}