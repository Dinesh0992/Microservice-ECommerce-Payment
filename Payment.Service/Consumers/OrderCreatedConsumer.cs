using MassTransit;
using ECommerce.Contracts;
using Razorpay.Api;

namespace Payment.Service.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly IConfiguration _config;
    public OrderCreatedConsumer(IConfiguration config) => _config = config;

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        // LOG 1: Arrival
        Console.WriteLine("\n================================================");
        Console.WriteLine($"[MESSAGE ARRIVED] Order: {context.Message.OrderId}");
        Console.WriteLine($"[MESSAGE ARRIVED] Email: {context.Message.CustomerEmail}");
        Console.WriteLine("================================================");

        try 
        {
            string key = _config["RazorPay:KeyId"]!;
            string secret = _config["RazorPay:KeySecret"]!;
            RazorpayClient client = new RazorpayClient(key, secret);

            Dictionary<string, object> options = new();
            options.Add("amount", (int)(context.Message.Amount * 100)); 
            options.Add("currency", "INR");
            options.Add("receipt", context.Message.OrderId.ToString());

            Console.WriteLine(">>> Contacting Razorpay API...");
            
            Order razorPayOrder = client.Order.Create(options);
            string rzpOrderId = razorPayOrder["id"].ToString();

            // LOG 2: Success
            Console.WriteLine($"[RAZORPAY SUCCESS] Created ID: {rzpOrderId}");
            Console.WriteLine("================================================\n");

            await context.Publish(new PaymentInitiated 
            { 
                OrderId = context.Message.OrderId, 
                RazorpayOrderId = rzpOrderId 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine($"[CONSUMER ERROR] {ex.Message}");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");
            throw; 
        }
    }
}