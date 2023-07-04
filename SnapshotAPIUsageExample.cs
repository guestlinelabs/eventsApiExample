//This code snippet contains C# code with an example of how Guestline Snapshot API can be used to:
// - Get URLs for the latest snapshot containing all the raw events for a given site and event stream (rooms)
// - Download the blob using the URL and process all the events streaming the blob content, deserializing and processing each event.
//
//NOTE: This code is not production ready code nor has been tested. It is just an example to illustrate how the API can be used, and should
//      be taken only as reference to build your own solutions as per your requirements.


//Register named http clients in Program.cs
//More info and different options: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
services.AddHttpClient(nameof(BlobReader))
    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });
services.AddHttpClient(nameof(SnapshotUriProvider),
    httpClient => 
    {
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "YOUR-API-KEY");
        httpClient.BaseAddress = new Uri("https://insights-snapshot-api.eu.guestline.app/api/bloburis/");
    });

//Classes modelling API response
public class SnapshotMetadata
{
    public int ItemCount { get; set; }
    public int SizeInBytes { get; set; }
}

public class SnapshotAPIResponse
{
    public string Snapshot { get; set; }
    public SnapshotMetadata SnapshotMetadata { get; set; }
    public string[] EventLogs { get; set; }
}

//Classes modelling events that downloaded blobs contain
//(Not all the event streams have the same format)
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

public class EventGridEnvelope<T>
{
    public T Details { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Room
{
    public string Id { get; set; }
    public string RoomClassDescription { get; set; }
    public bool RFlag { get; set; }

    public bool IsValidBedroom => !RFlag && RoomClassDescription == "Bedroom";
}

//Example on how to call SnapshotAPI to get snapshot URI
public class SnapshotUriProvider
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public SnapshotUriProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };  
    }

    public async Task<Uri> GetUriAsync(string groupId, string siteId, string eventStream)
    {
        var snapshotApiUri = new Uri($"{eventStream}/{groupId}/{siteId}", UriKind.Relative);
        var httpClient = _httpClientFactory.CreateClient(nameof(SnapshotUriProvider));
        var response = await httpClient.GetAsync(snapshotApiUri);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        var content = JsonSerializer.Deserialize<SnapshotAPIResponse>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);
        return new Uri(content!.Snapshot);
    }
}

//Example on how to download a blob (streaming events)
public class BlobReader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public BlobReader(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async IAsyncEnumerable<T> ReadAsync<T>(Uri blobUri)
    {
        if (blobUri == null) yield break;

        var httpClient = _httpClientFactory.CreateClient(nameof(BlobReader));

        using var responseStream = await httpClient.GetStreamAsync(blobUri);
        using var streamReader = new StreamReader(responseStream);
        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync();                
            var blobEvent = JsonSerializer.Deserialize<T>(line!, _jsonSerializerOptions);
            yield return blobEvent;
        }
    }
}

//Usage example
public class EventProcessor 
{
    private readonly SnapshotUriProvider _snapshotUriProvider;
    private readonly BlobReader _blobReader;

    public BlobReader(SnapshotUriProvider snapshotUriProvider, BlobReader blobReader)
    {
        _snapshotUriProvider = snapshotUriProvider;
        _blobReader = blobReader;
    }

    public async Task<int> CountTotalValidBedrooms()
    {
        var roomsSnapshotUri = await _snapshotUriProvider.GetUriAsync("GroupId", "SiteId", "rooms");
        var totalValidBedrooms = 0;
        await foreach (var roomEvent in _blobReader.ReadAsync<EventGridEvent<Room>>(roomsSnapshotUri))
        {
            if (roomEvent.IsValidBedroom) totalValidBedrooms++;
        }
        return totalValidBedrooms;
    }
}