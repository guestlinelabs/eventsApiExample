using EventsAPIConsumer.Events;
using EventsAPIConsumer.MetricCalculators;
using EventsAPIConsumer.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

var eventsApiSettings = configuration.GetSection("EventsApi").Get<EventsApiSettings>();

services.AddSingleton<IEventsUriProvider, EventsUriProvider>();
services.AddSingleton<ISnapshotReader, SnapshotReader>();
services.AddSingleton<RoomStockCalculator, RoomStockCalculator>();

//Http client for SnapshotReader
services.AddHttpClient(nameof(SnapshotReader))
    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });
//Http client for EventsUriProvider
services.AddHttpClient(nameof(EventsUriProvider),
    httpClient =>
    {
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", eventsApiSettings.ApiKey);
        httpClient.BaseAddress = new Uri(eventsApiSettings.BlobUrisEndpoint);
    });

var serviceProvider = services.BuildServiceProvider();

var roomStockCalculator = serviceProvider.GetRequiredService<RoomStockCalculator>();

Console.WriteLine("Enter GroupId: ");
var groupId = Console.ReadLine();
Console.WriteLine("Enter SiteId: ");
var siteId = Console.ReadLine();

var totalValidRooms = await roomStockCalculator.CountTotalValidBedrooms(groupId, siteId);

Console.WriteLine($"Site {groupId}-{siteId} has {totalValidRooms} valid rooms.");
