using MQTTnet;
using MQTTnet.Client;
using NhsWales.TemperatureMonitor.Domain.Models; 
using NhsWales.TemperatureMonitor.Infrastructure.Options;
using System.Text.Json;

namespace NhsWales.TemperatureMonitor.Infrastructure.Mqtt;

public sealed class MqttTemperatureSubscriber : IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttOptions _options;

    public MqttTemperatureSubscriber(IMqttClientFactory factory, MqttOptions options)
    {
        _options = options;
        _client = factory.CreateClient();
    }

    public async Task StartAsync(Func<TemperatureReading, CancellationToken, Task> onReading, CancellationToken ct)
    {
        _client.ApplicationMessageReceivedAsync += e =>
        {
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            var dto = JsonSerializer.Deserialize<MqttTempDto>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dto is null) return Task.CompletedTask;

            var reading = TemperatureReading.Create(
                dto.DeviceId ?? "unknown",
                dto.Celsius,
                dto.TimestampUtc ?? DateTimeOffset.UtcNow);

            return onReading(reading, ct);
        };

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithCleanSession()
            .Build();

        await _client.ConnectAsync(mqttClientOptions, ct).ConfigureAwait(false);
        await _client.SubscribeAsync(_options.Topic, cancellationToken: ct).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_client.IsConnected)
                await _client.DisconnectAsync();
        }
        catch { /* ignore */ }

        _client.Dispose();
    }

    private sealed record MqttTempDto(string? DeviceId, double Celsius, DateTimeOffset? TimestampUtc);
}
