using ECommerce.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;

namespace Payment.Service.Controllers;

[ApiController]
// This ensures the route is api/payments/ConfirmPayment
[Route("api/[controller]/[action]")] 
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentsController(IConfiguration configuration, IPublishEndpoint publishEndpoint)
    {
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentVerificationRequest request)
    {
        try
        {
            // 1. Get secret from configuration 
            string secret = _configuration["RazorPay:KeySecret"] ?? "YOUR_SECRET_HERE";

            // 2. Construct payload exactly as Razorpay expects [cite: 4, 9]
            string payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";

            // 3. Verify Signature [cite: 4, 9]
            Utils.verifyWebhookSignature(payload, request.RazorpaySignature, secret);

            // 4. Verification passed -> Publish to RabbitMQ [cite: 10, 11]
            await _publishEndpoint.Publish(new PaymentCompleted
            {
                OrderId = request.OrderId,
                RazorpayPaymentId = request.RazorpayPaymentId,
                RazorpayOrderId = request.RazorpayOrderId
            });

            return Ok(new { Message = "Payment successful! Order update is processing." });
        }
        catch (Razorpay.Api.Errors.SignatureVerificationError)
        {
            return BadRequest(new { Message = "Invalid payment signature." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred: " + ex.Message });
        }
    }
}

// Keep this class name matching your frontend's JSON structure [cite: 7]
public class PaymentVerificationRequest
{
    public required string RazorpayOrderId { get; set; }
    public required string RazorpayPaymentId { get; set; }
    public required string RazorpaySignature { get; set; }
    public required Guid OrderId { get; set; } 
}