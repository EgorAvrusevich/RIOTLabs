using OnlineStore.Shared.Models;

namespace OnlineStore.Server.Data;

public static class InMemoryStore
{
    public static List<Customer> Customers { get; } = new()
    {
        new Customer { Id = 1, Name = "Иван Иванов", Email = "ivan@mail.ru" },
        new Customer { Id = 2, Name = "Петр Петров", Email = "petr@mail.ru" }
    };

    public static List<Product> Products { get; } = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 50000, StockQuantity = 10 },
        new Product { Id = 2, Name = "Mouse", Price = 1500, StockQuantity = 50 },
        new Product { Id = 3, Name = "Keyboard", Price = 3000, StockQuantity = 30 }
    };

    public static List<Order> Orders { get; } = new();
    public static List<Payment> Payments { get; } = new();

    private static int _orderId = 0;
    private static int _paymentId = 0;

    public static int NextOrderId() => ++_orderId;
    public static int NextPaymentId() => ++_paymentId;
}
