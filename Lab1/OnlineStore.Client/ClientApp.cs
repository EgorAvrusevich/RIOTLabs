using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using OnlineStore.Shared.Models;
using OnlineStore.Shared.Protocol;

namespace OnlineStore.Client;

public class ClientApp
{
    private readonly string _host;
    private readonly int _port;

    public ClientApp(string host = "127.0.0.1", int port = 5000)
    {
        _host = host;
        _port = port;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Клиент Интернет-магазина ===");
        Console.WriteLine($"Подключение к {_host}:{_port}...");

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port);
            Console.WriteLine("Подключение установлено.\n");

            using var stream = client.GetStream();

            while (true)
            {
                PrintMenu();
                var choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    Console.WriteLine("Отключение...");
                    break;
                }

                Request? request = choice switch
                {
                    "1" => BuildGetProductsRequest(),
                    "2" => BuildCreateOrderRequest(),
                    "3" => BuildCancelOrderRequest(),
                    "4" => BuildPayOrderRequest(),
                    _ => null
                };

                if (request == null)
                {
                    Console.WriteLine("Некорректный выбор. Попробуйте снова.\n");
                    continue;
                }

                var response = await SendRequestAsync(stream, request);
                PrintResponse(response);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Ошибка: сервер недоступен ({ex.Message})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сетевого взаимодействия: {ex.Message}");
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("Выберите операцию:");
        Console.WriteLine("1. Получить список товаров (GetProducts)");
        Console.WriteLine("2. Создать заказ (CreateOrder)");
        Console.WriteLine("3. Отменить заказ (CancelOrder)");
        Console.WriteLine("4. Оплатить заказ (PayOrder)");
        Console.WriteLine("0. Выход");
        Console.Write("> ");
    }

    private Request BuildGetProductsRequest()
    {
        return new Request { Operation = "GetProducts", Payload = new Dictionary<string, object>() };
    }

    private Request BuildCreateOrderRequest()
    {
        Console.Write("ID клиента: ");
        var customerId = int.Parse(Console.ReadLine()!);

        var items = new List<OrderItem>();
        while (true)
        {
            Console.Write("ID товара (или 0 для завершения): ");
            var productId = int.Parse(Console.ReadLine()!);
            if (productId == 0) break;

            Console.Write("Количество: ");
            var quantity = int.Parse(Console.ReadLine()!);

            items.Add(new OrderItem { ProductId = productId, Quantity = quantity });
        }

        return new Request
        {
            Operation = "CreateOrder",
            Payload = new Dictionary<string, object>
            {
                ["customerId"] = customerId,
                ["items"] = items
            }
        };
    }

    private Request BuildCancelOrderRequest()
    {
        Console.Write("ID заказа: ");
        var orderId = int.Parse(Console.ReadLine()!);

        return new Request
        {
            Operation = "CancelOrder",
            Payload = new Dictionary<string, object> { ["orderId"] = orderId }
        };
    }

    private Request BuildPayOrderRequest()
    {
        Console.Write("ID заказа: ");
        var orderId = int.Parse(Console.ReadLine()!);
        Console.Write("Сумма оплаты: ");
        var amount = decimal.Parse(Console.ReadLine()!);

        return new Request
        {
            Operation = "PayOrder",
            Payload = new Dictionary<string, object>
            {
                ["orderId"] = orderId,
                ["amount"] = amount
            }
        };
    }

    private async Task<Response> SendRequestAsync(NetworkStream stream, Request request)
    {
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var bytes = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(bytes, 0, bytes.Length);

        var buffer = new byte[8192];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        return JsonSerializer.Deserialize<Response>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private void PrintResponse(Response response)
    {
        Console.WriteLine($"\n--- Ответ сервера ---");
        Console.WriteLine($"Успех: {response.Success}");
        Console.WriteLine($"Сообщение: {response.Message}");

        if (response.Data != null)
        {
            Console.WriteLine($"Данные: {JsonSerializer.Serialize(response.Data, new JsonSerializerOptions { WriteIndented = true })}");
        }

        if (response.Events.Any())
        {
            Console.WriteLine($"События: {string.Join(", ", response.Events)}");
        }

        Console.WriteLine("---------------------\n");
    }
}
