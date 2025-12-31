using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FIX for FirstOrDefaultAsync
using Order.Service.Data;
using Order.Service.Models;
using ECommerce.Contracts;
using MassTransit;


namespace Order.Service.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public OrdersController(
        OrderDbContext dbContext,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        var order = new Orders
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _dbContext.Orders.Add(order);

        await _publishEndpoint.Publish(new OrderCreated
        {
            OrderId = order.Id,
            Amount = order.Amount,
            CustomerEmail = request.CustomerEmail
        });

        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Order Processed", OrderId = order.Id });
    }

    [HttpGet("{id}")]
    // URL: GET api/Orders/GetOrder/guid-id-here
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost("{orderId}")] // Added "{orderId}" to accept the ID from the URL path
    public async Task<IActionResult> CancelOrder(Guid orderId) 
    {
        // 1. Find the order using Guid
        var order = await _dbContext.Orders.FindAsync(orderId);

        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        // 2. Security Check
        if (order.Status == "PaymentInitiated" || order.Status == "Pending")
        {
            order.Status = "Cancelled";
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"[STATUS UPDATE] Order {orderId} has been cancelled by user.");
            return Ok(new { message = "Order cancelled" });
        }

        return BadRequest(new { message = "Order cannot be cancelled at this stage." });
    }
}

public record OrderRequest(decimal Amount, string CustomerEmail);