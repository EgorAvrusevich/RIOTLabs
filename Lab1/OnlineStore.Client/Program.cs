namespace OnlineStore.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var app = new ClientApp();
        await app.RunAsync();
    }
}
