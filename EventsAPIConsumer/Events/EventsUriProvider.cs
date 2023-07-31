using EventsAPIConsumer.Events.Models;
using System.Net;
using System.Text.Json;

namespace EventsAPIConsumer.Events;

public interface IEventsUriProvider
{
    Task<Uri> GetSnapshotUriAsync(string groupId, string siteId, string eventStream);
}

public class EventsUriProvider : IEventsUriProvider
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public EventsUriProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Uri> GetSnapshotUriAsync(string groupId, string siteId, string eventStream)
    {
        var snapshotApiUri = new Uri($"{eventStream}/{groupId}/{siteId}", UriKind.Relative);
        var httpClient = _httpClientFactory.CreateClient(nameof(EventsUriProvider));
        var response = await httpClient.GetAsync(snapshotApiUri);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        var content = JsonSerializer.Deserialize<EventsAPIResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);
        return new Uri(content!.Snapshot);
    }
}
