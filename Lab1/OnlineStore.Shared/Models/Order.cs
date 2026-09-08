using System.Text.Json.Serialization;

namespace OnlineStore.Shared.Models;

public enum OrderStatus
{
    Created,
    Paid,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}
