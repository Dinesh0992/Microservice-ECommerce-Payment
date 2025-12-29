using MassTransit;
using ECommerce.Contracts;
using Order.Service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR; // Added for SignalR
using Order.Service.Hubs;          // Added to access your Hub class

namespace Order.Service.Consumers;

public class PaymentInitiatedConsumer : IConsumer<PaymentInitiated>
{
    private readonly OrderDbContext _dbContext;
    private readonly IHubContext<OrderHub> _hubContext; // Added field for Hub context

    public PaymentInitiatedConsumer(OrderDbContext dbContext, IHubContext<OrderHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
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

           // 4. SIGNALR PUSH: Notify the frontend browser
            // We send the RazorpayOrderId to the specific Group named after the Order ID
            await _hubContext.Clients.Group(order.Id.ToString())
                .SendAsync("UpdateRazorpayId", order.RazorpayOrderId);

            Console.WriteLine($"[SIGNALR SENT] Order {order.Id} notification pushed to client.");
            Console.WriteLine($"[DATABASE UPDATED] Order {order.Id} now linked to Razorpay: {order.RazorpayOrderId}");
        }
    }
}