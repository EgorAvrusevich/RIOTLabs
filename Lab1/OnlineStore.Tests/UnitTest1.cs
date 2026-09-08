using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using OnlineStore.Client;
using OnlineStore.Server.Data;
using OnlineStore.Server.Handlers;
using OnlineStore.Server.Services;
using OnlineStore.Shared.Models;
using OnlineStore.Shared.Protocol;
using Xunit;

namespace OnlineStore.Tests;

public class UnitTest1
{
    public UnitTest1()
    {
        // Сброс хранилища перед каждым тестом
        InMemoryStore.Orders.Clear();
        InMemoryStore.Payments.Clear();
        foreach (var p in InMemoryStore.Products)
        {
            p.StockQuantity = p.Id switch
            {
                1 => 10,
                2 => 50,
                3 => 30,
                _ => p.StockQuantity
            };
        }
    }

    /// <summary>
    /// Тест 1: Успешный сценарий создания заказа
    /// </summary>
    [Fact]
    public void Test1_CreateOrder_Success()
    {
        var service = new OrderService();
        var items = new List<OrderItem>
        {
            new() { ProductId = 1, Quantity = 2 }
        };

        var (order, events) = service.CreateOrder(1, items);

        Assert.NotNull(order);
        Assert.Equal(1, order.CustomerId);
        Assert.Equal(100000m, order.TotalAmount); // 2 * 50000
        Assert.Contains(events, e => e.Name == "OrderCreated");
        Assert.Contains(events, e => e.Name == "StockUpdated");
    }

    /// <summary>
    /// Тест 2: Неизвестная операция
    /// </summary>
    [Fact]
    public void Test2_UnknownOperation_ReturnsError()
    {
        var handler = new OperationHandler();
        var request = new Request
        {
            Operation = "DeleteEverything",
            Payload = new Dictionary<string, object>()
        };

        var response = handler.Handle(request);

        Assert.False(response.Success);
        Assert.Contains("Неизвестная операция", response.Message);
    }

    /// <summary>
    /// Тест 3: Некорректные данные (недостаточно товара на складе)
    /// </summary>
    [Fact]
    public void Test3_CreateOrder_InsufficientStock_ReturnsError()
    {
        var handler = new OperationHandler();
        var request = new Request
        {
            Operation = "CreateOrder",
            Payload = new Dictionary<string, object>
            {
                ["customerId"] = 1,
                ["items"] = new List<OrderItem>
                {
                    new() { ProductId = 1, Quantity = 999 }
                }
            }
        };

        var response = handler.Handle(request);

        Assert.False(response.Success);
        Assert.Contains("Недостаточно", response.Message);
    }

    /// <summary>
    /// Тест 4: Недоступный сервер
    /// </summary>
    [Fact]
    public async Task Test4_ServerUnavailable_ThrowsException()
    {
        var client = new TcpClient();
        var ex = await Assert.ThrowsAsync<SocketException>(async () =>
        {
            await client.ConnectAsync(IPAddress.Loopback, 59999); // порт, на котором нет сервера
        });

        Assert.Equal(SocketError.ConnectionRefused, ex.SocketErrorCode);
    }

    /// <summary>
    /// Дополнительный тест: отмена заказа возвращает события
    /// </summary>
    [Fact]
    public void Test5_CancelOrder_ReturnsEvents()
    {
        var service = new OrderService();
        var items = new List<OrderItem> { new() { ProductId = 2, Quantity = 5 } };
        var (order, _) = service.CreateOrder(1, items);

        var (_, events) = service.CancelOrder(order.Id);

        Assert.Contains(events, e => e.Name == "OrderCancelled");
        Assert.Contains(events, e => e.Name == "StockUpdated");
    }

    /// <summary>
    /// Дополнительный тест: оплата заказа
    /// </summary>
    [Fact]
    public void Test6_PayOrder_Success()
    {
        var service = new OrderService();
        var items = new List<OrderItem> { new() { ProductId = 3, Quantity = 1 } };
        var (order, _) = service.CreateOrder(2, items);

        var (payment, events) = service.PayOrder(order.Id, 3000m);

        Assert.NotNull(payment);
        Assert.Contains(events, e => e.Name == "PaymentCompleted");
    }
}
