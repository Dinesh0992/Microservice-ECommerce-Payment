namespace ECommerce.Contracts;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public  required string CustomerEmail { get; init; }
}