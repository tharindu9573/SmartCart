using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class EventHubConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventHubConsumerService> _logger;

    public EventHubConsumerService(
        IConfiguration config,
        IServiceScopeFactory scopeFactory,
        ILogger<EventHubConsumerService> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _config["EventHub:ConnectionString"];
        var eventHubName = _config["EventHub:Name"];
        var consumerGroup = _config["EventHub:ConsumerGroup"] ?? EventHubConsumerClient.DefaultConsumerGroupName;

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
        {
            _logger.LogWarning("EventHub not configured. Background consumer will not start.");
            return;
        }

        _logger.LogInformation("EventHub consumer starting. Hub={Hub} Group={Group}", eventHubName, consumerGroup);

        await using var consumer = new EventHubConsumerClient(consumerGroup, connectionString, eventHubName);

        try
        {
            await foreach (var partitionEvent in consumer.ReadEventsAsync(startReadingAtEarliestEvent: false, cancellationToken: stoppingToken))
            {
                try
                {
                    var body = partitionEvent.Data.EventBody.ToString();
                    _logger.LogDebug("EventHub message received: {Body}", body);

                    var payload = JsonConvert.DeserializeObject<ScanEventPayload>(body);
                    if (payload == null) continue;

                    using var scope = _scopeFactory.CreateScope();
                    var scanService = scope.ServiceProvider.GetRequiredService<ICartScanService>();
                    await scanService.ProcessScanEventAsync(payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing EventHub message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("EventHub consumer stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EventHub consumer encountered a fatal error.");
        }
    }
}
