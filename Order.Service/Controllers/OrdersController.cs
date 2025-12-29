using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FIX for FirstOrDefaultAsync
using Order.Service.Data;
using Order.Service.Models;
using ECommerce.Contracts;
using MassTransit;
using Razorpay.Api; // FIX for Utils
using System.Collections.Generic;

namespace Order.Service.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration; // 1. DECLARE THIS

    // 2. ADD IConfiguration TO THE CONSTRUCTOR
    public OrdersController(
        OrderDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration; // 3. ASSIGN IT
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

    [HttpPost]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentVerificationRequest request)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync<Orders>(o => o.RazorpayOrderId == request.RazorpayOrderId);

        if (order == null) return NotFound(new { Message = "Order not found." });

        try
        {
            // 1. Get your secret
            string secret = _configuration["Razorpay:Secret"] ?? "YOUR_SECRET_HERE";

            // 2. Manually construct the payload as Razorpay expects: "order_id|payment_id"
            // This is exactly what the internal SDK code does.
            string payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";

            // 3. Use the 3-argument method which IS NOT read-only dependent
            Utils.verifyWebhookSignature(payload, request.RazorpaySignature, secret);

            // 4. If we reach here, verification passed
            order.Status = "Paid";
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Payment successful! Order marked as Paid." });
        }
        catch (Razorpay.Api.Errors.SignatureVerificationError)
        {
            return BadRequest(new { Message = "Invalid payment signature." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred during verification." + ex.Message });
        }
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
}

public record OrderRequest(decimal Amount, string CustomerEmail);