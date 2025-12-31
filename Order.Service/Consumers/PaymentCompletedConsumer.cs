using ECommerce.Contracts;
using MassTransit;
using Order.Service.Data;

namespace Order.Service.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
{
    private readonly OrderDbContext _dbContext;

    public PaymentCompletedConsumer(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<PaymentCompleted> context)
    {
        // Find the order by internal ID 
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);

        if (order != null)
        {
            // Update status to Paid 
            order.Status = "Paid";
            await _dbContext.SaveChangesAsync();
        }
    }
}