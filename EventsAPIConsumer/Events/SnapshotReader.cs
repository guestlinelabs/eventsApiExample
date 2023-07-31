using System.Text.Json;

namespace EventsAPIConsumer.Events;

public interface ISnapshotReader
{
    IAsyncEnumerable<T> ReadAsync<T>(Uri blobUri);
}

public class SnapshotReader : ISnapshotReader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SnapshotReader(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async IAsyncEnumerable<T> ReadAsync<T>(Uri blobUri)
    {
        if (blobUri == null) yield break;

        var httpClient = _httpClientFactory.CreateClient(nameof(SnapshotReader));

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
