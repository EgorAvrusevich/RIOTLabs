using OnlineStore.Server.Data;
using OnlineStore.Shared.Events;
using OnlineStore.Shared.Models;

namespace OnlineStore.Server.Services;

public class OrderService : IOrderService
{
    public (Order order, List<DomainEvent> events) CreateOrder(int customerId, List<OrderItem> items)
    {
        var customer = InMemoryStore.Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
            throw new ArgumentException($"Клиент с ID={customerId} не найден");

        if (items == null || items.Count == 0)
            throw new ArgumentException("Заказ должен содержать хотя бы один товар");

        foreach (var item in items)
        {
            var product = InMemoryStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                throw new ArgumentException($"Товар с ID={item.ProductId} не найден");
            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Недостаточно товара '{product.Name}' на складе (доступно: {product.StockQuantity}, требуется: {item.Quantity})");

            item.UnitPrice = product.Price;
            product.StockQuantity -= item.Quantity;
        }

        var order = new Order
        {
            Id = InMemoryStore.NextOrderId(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        InMemoryStore.Orders.Add(order);

        var events = new List<DomainEvent>
        {
            new DomainEvent("OrderCreated", new Dictionary<string, object>
            {
                ["OrderId"] = order.Id,
                ["CustomerId"] = customerId,
                ["TotalAmount"] = order.TotalAmount
            }),
            new DomainEvent("StockUpdated", new Dictionary<string, object>
            {
                ["Products"] = items.Select(i => new { i.ProductId, i.Quantity }).ToList()
            })
        };

        return (order, events);
    }

    public (Order? order, List<DomainEvent> events) CancelOrder(int orderId)
    {
        var order = InMemoryStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            throw new ArgumentException($"Заказ с ID={orderId} не найден");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Заказ уже отменен");

        if (order.Status == OrderStatus.Paid)
            throw new InvalidOperationException("Нельзя отменить оплаченный заказ");

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.Items)
        {
            var product = InMemoryStore.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
                product.StockQuantity += item.Quantity;
        }

        var events = new List<DomainEvent>
        {
            new DomainEvent("OrderCancelled", new Dictionary<string, object>
            {
                ["OrderId"] = orderId,
                ["Reason"] = "Отмена пользователем"
            }),
            new DomainEvent("StockUpdated", new Dictionary<string, object>
            {
                ["Products"] = order.Items.Select(i => new { i.ProductId, Quantity = i.Quantity }).ToList()
            })
        };

        return (order, events);
    }

    public (Payment? payment, List<DomainEvent> events) PayOrder(int orderId, decimal amount)
    {
        var order = InMemoryStore.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null)
            throw new ArgumentException($"Заказ с ID={orderId} не найден");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Нельзя оплатить отмененный заказ");

        if (order.Status == OrderStatus.Paid)
            throw new InvalidOperationException("Заказ уже оплачен");

        if (amount < order.TotalAmount)
            throw new InvalidOperationException($"Недостаточная сумма оплаты (требуется: {order.TotalAmount}, получено: {amount})");

        var payment = new Payment
        {
            Id = InMemoryStore.NextPaymentId(),
            OrderId = orderId,
            Amount = amount,
            PaidAt = DateTime.UtcNow
        };

        InMemoryStore.Payments.Add(payment);
        order.Status = OrderStatus.Paid;

        var events = new List<DomainEvent>
        {
            new DomainEvent("PaymentCompleted", new Dictionary<string, object>
            {
                ["PaymentId"] = payment.Id,
                ["OrderId"] = orderId,
                ["Amount"] = amount
            })
        };

        return (payment, events);
    }
}
