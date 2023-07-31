namespace EventsAPIConsumer.Events.Models;

public class EventGridEnvelope<T>
{
    public T Details { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
}
