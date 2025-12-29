namespace ECommerce.Contracts;

public record PaymentInitiated
{
    public Guid OrderId { get; init; }
    public required string RazorpayOrderId { get; init; }
}