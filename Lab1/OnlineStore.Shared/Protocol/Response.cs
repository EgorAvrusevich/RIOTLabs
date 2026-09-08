namespace OnlineStore.Shared.Protocol;

public class Response
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public List<string> Events { get; set; } = new();
}
