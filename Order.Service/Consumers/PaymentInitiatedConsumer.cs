using MassTransit;
using ECommerce.Contracts;
using Order.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Order.Service.Consumers;

public class PaymentInitiatedConsumer : IConsumer<PaymentInitiated>
{
    private readonly OrderDbContext _dbContext;

    public PaymentInitiatedConsumer(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<PaymentInitiated> context)
    {
        // 1. Find the order in the database
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == context.Message.OrderId);

        if (order != null)
        {
            // 2. Update the fields
            order.RazorpayOrderId = context.Message.RazorpayOrderId;
            order.Status = "PaymentInitiated"; // Update status to reflect progress

            // 3. Save changes
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"[DATABASE UPDATED] Order {order.Id} now linked to Razorpay: {order.RazorpayOrderId}");
        }
    }
}