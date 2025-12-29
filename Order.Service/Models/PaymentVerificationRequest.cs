namespace Order.Service.Models;

public class PaymentVerificationRequest
{
    public required string RazorpayOrderId { get; set; }
    public required string RazorpayPaymentId { get; set; }
    public required string RazorpaySignature { get; set; }
}