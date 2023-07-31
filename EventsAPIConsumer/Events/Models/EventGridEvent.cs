namespace EventsAPIConsumer.Events.Models;

public class EventGridEvent<T>
{
    public string Id { get; set; }
    public string Topic { get; set; }
    public string Subject { get; set; }
    public EventGridEnvelope<T> Data { get; set; }
    public string EventType { get; set; }
    public DateTime EventTime { get; set; }
    public string MetadataVersion { get; }
    public string DataVersion { get; set; }
}
