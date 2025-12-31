namespace ECommerce.Contracts;

public record PaymentCompleted
{
    public Guid OrderId { get; init; }
    public required string RazorpayPaymentId { get; init; }
    public required string RazorpayOrderId { get; init; }
}