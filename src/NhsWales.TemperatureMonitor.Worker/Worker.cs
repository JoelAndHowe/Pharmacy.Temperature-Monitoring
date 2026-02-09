using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NhsWales.TemperatureMonitor.Application.UseCases;
using NhsWales.TemperatureMonitor.Infrastructure.Mqtt;

namespace NhsWales.TemperatureMonitor.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MqttTemperatureSubscriber _subscriber;
    private readonly ITemperatureIngestUseCase _useCase;

    public Worker(
        ILogger<Worker> logger,
        MqttTemperatureSubscriber subscriber,
        ITemperatureIngestUseCase useCase)
    {
        _logger = logger;
        _subscriber = subscriber;
        _useCase = useCase;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MQTT subscriber...");

        await _subscriber.StartAsync(async (reading, ct) =>
        {
            _logger.LogInformation("Received {DeviceId} {Temp}°C @ {Time}",
                reading.DeviceId, reading.Celsius, reading.TimestampUtc);

            await _useCase.ExecuteAsync(reading, ct);
        }, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _subscriber.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
