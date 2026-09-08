using OnlineStore.Shared.Events;
using OnlineStore.Shared.Models;

namespace OnlineStore.Server.Services;

public interface IOrderService
{
    (Order order, List<DomainEvent> events) CreateOrder(int customerId, List<OrderItem> items);
    (Order? order, List<DomainEvent> events) CancelOrder(int orderId);
    (Payment? payment, List<DomainEvent> events) PayOrder(int orderId, decimal amount);
}
