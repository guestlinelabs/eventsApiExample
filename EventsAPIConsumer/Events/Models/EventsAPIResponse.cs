namespace EventsAPIConsumer.Events.Models;

public class EventsAPIResponse
{
    public string Snapshot { get; set; }
    public SnapshotMetadata SnapshotMetadata { get; set; }
    public string[] EventLogs { get; set; }
}

public class SnapshotMetadata
{
    public int ItemCount { get; set; }
    public int SizeInBytes { get; set; }
}
