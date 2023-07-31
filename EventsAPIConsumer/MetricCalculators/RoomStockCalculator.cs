using EventsAPIConsumer.Events;
using EventsAPIConsumer.Events.Models;

namespace EventsAPIConsumer.MetricCalculators;

public class RoomStockCalculator
{
    private readonly IEventsUriProvider _eventsUriProvider;
    private readonly ISnapshotReader _snapshotReader;

    public RoomStockCalculator(IEventsUriProvider eventsUriProvider, ISnapshotReader snapshotReader)
    {
        _eventsUriProvider = eventsUriProvider;
        _snapshotReader = snapshotReader;
    }

    public async Task<int> CountTotalValidBedrooms(string groupId, string siteId)
    {
        var roomsSnapshotUri = await _eventsUriProvider.GetSnapshotUriAsync(groupId, siteId, EventStreams.Rooms);
        var totalValidBedrooms = 0;
        await foreach (var roomEvent in _snapshotReader.ReadAsync<EventGridEvent<Room>>(roomsSnapshotUri))
        {
            if (roomEvent.Data.Details.IsValidBedroom) totalValidBedrooms++;
        }
        return totalValidBedrooms;
    }
}
