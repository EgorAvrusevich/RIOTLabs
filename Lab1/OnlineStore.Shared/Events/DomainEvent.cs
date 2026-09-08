namespace OnlineStore.Shared.Events;

public class DomainEvent
{
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Payload { get; set; } = new();

    public DomainEvent(string name, Dictionary<string, object> payload)
    {
        Name = name;
        Timestamp = DateTime.UtcNow;
        Payload = payload;
    }
}
