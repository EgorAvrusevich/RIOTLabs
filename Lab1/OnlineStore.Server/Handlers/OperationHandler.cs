using System.Text.Json;
using OnlineStore.Server.Data;
using OnlineStore.Server.Services;
using OnlineStore.Shared.Events;
using OnlineStore.Shared.Models;
using OnlineStore.Shared.Protocol;

namespace OnlineStore.Server.Handlers;

public class OperationHandler
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    public OperationHandler()
    {
        _orderService = new OrderService();
        _productService = new ProductService();
    }

    public Response Handle(Request request)
    {
        try
        {
            return request.Operation.ToLower() switch
            {
                "createorder" => HandleCreateOrder(request.Payload),
                "cancelorder" => HandleCancelOrder(request.Payload),
                "getproducts" => HandleGetProducts(),
                "payorder" => HandlePayOrder(request.Payload),
                _ => new Response
                {
                    Success = false,
                    Message = $"Неизвестная операция: '{request.Operation}'"
                }
            };
        }
        catch (ArgumentException ex)
        {
            return new Response { Success = false, Message = $"Ошибка данных: {ex.Message}" };
        }
        catch (InvalidOperationException ex)
        {
            return new Response { Success = false, Message = $"Ошибка операции: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new Response { Success = false, Message = $"Внутренняя ошибка: {ex.Message}" };
        }
    }

    private Response HandleCreateOrder(Dictionary<string, object> payload)
    {
        var customerId = GetInt(payload, "customerId");
        var itemsJson = JsonSerializer.Serialize(payload["items"]);
        var items = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var (order, events) = _orderService.CreateOrder(customerId, items!);

        return new Response
        {
            Success = true,
            Message = $"Заказ #{order.Id} создан. Сумма: {order.TotalAmount:C}",
            Data = order,
            Events = events.Select(e => e.Name).ToList()
        };
    }

    private Response HandleCancelOrder(Dictionary<string, object> payload)
    {
        var orderId = GetInt(payload, "orderId");
        var (order, events) = _orderService.CancelOrder(orderId);

        return new Response
        {
            Success = true,
            Message = $"Заказ #{orderId} отменен",
            Data = order,
            Events = events.Select(e => e.Name).ToList()
        };
    }

    private Response HandleGetProducts()
    {
        var products = _productService.GetProducts();
        return new Response
        {
            Success = true,
            Message = $"Получено {products.Count} товаров",
            Data = products,
            Events = new List<string>()
        };
    }

    private Response HandlePayOrder(Dictionary<string, object> payload)
    {
        var orderId = GetInt(payload, "orderId");
        var amount = GetDecimal(payload, "amount");
        var (payment, events) = _orderService.PayOrder(orderId, amount);

        return new Response
        {
            Success = true,
            Message = $"Оплата заказа #{orderId} выполнена. Платеж #{payment!.Id}",
            Data = payment,
            Events = events.Select(e => e.Name).ToList()
        };
    }

    private static int GetInt(Dictionary<string, object> payload, string key)
    {
        if (!payload.TryGetValue(key, out var value))
            throw new ArgumentException($"Отсутствует обязательный параметр: {key}");

        return value switch
        {
            JsonElement json => json.GetInt32(),
            int i => i,
            long l => (int)l,
            _ => Convert.ToInt32(value)
        };
    }

    private static decimal GetDecimal(Dictionary<string, object> payload, string key)
    {
        if (!payload.TryGetValue(key, out var value))
            throw new ArgumentException($"Отсутствует обязательный параметр: {key}");

        return value switch
        {
            JsonElement json => json.GetDecimal(),
            decimal d => d,
            double dbl => (decimal)dbl,
            int i => i,
            _ => Convert.ToDecimal(value)
        };
    }
}
