namespace Order.Service.Models;

public class Orders
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }

    public string? RazorpayOrderId { get; set; }
}