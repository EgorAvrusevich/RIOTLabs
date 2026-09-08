using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using OnlineStore.Server.Handlers;
using OnlineStore.Shared.Protocol;

namespace OnlineStore.Server;

class Program
{
    private const int Port = 5000;

    static async Task Main(string[] args)
    {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"Сервер запущен на порту {Port}. Ожидание подключений...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Клиент подключен: {endpoint}");

        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[8192];

            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получено: {json}");

                Request? request;
                try
                {
                    request = JsonSerializer.Deserialize<Request>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    var errorResponse = new Response
                    {
                        Success = false,
                        Message = $"Некорректный JSON: {ex.Message}"
                    };
                    await SendResponse(stream, errorResponse);
                    continue;
                }

                if (request == null || string.IsNullOrWhiteSpace(request.Operation))
                {
                    var errorResponse = new Response
                    {
                        Success = false,
                        Message = "Некорректный запрос: отсутствует операция"
                    };
                    await SendResponse(stream, errorResponse);
                    continue;
                }

                var handler = new OperationHandler();
                var response = handler.Handle(request);
                await SendResponse(stream, response);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Ошибка сети с {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Ошибка обработки {endpoint}: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Клиент отключен: {endpoint}");
        }
    }

    static async Task SendResponse(NetworkStream stream, Response response)
    {
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var bytes = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(bytes, 0, bytes.Length);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Отправлено: {json}");
    }
}
