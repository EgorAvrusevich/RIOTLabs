namespace OnlineStore.Shared.Protocol;

public class Request
{
    public string Operation { get; set; } = string.Empty;
    public Dictionary<string, object> Payload { get; set; } = new();
}
