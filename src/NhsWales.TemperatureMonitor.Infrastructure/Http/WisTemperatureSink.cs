using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NhsWales.TemperatureMonitor.Application.Ports;
using NhsWales.TemperatureMonitor.Domain.Models;
using NhsWales.TemperatureMonitor.Infrastructure.Options;

namespace NhsWales.TemperatureMonitor.Infrastructure.Http;

public sealed class WisTemperatureSink : ITemperatureLeafSink
{
    private readonly HttpClient _http;
    private readonly IOptionsMonitor<HttpEndpointOptions> _options;

    public WisTemperatureSink(HttpClient http, IOptionsMonitor<HttpEndpointOptions> options)
    {
        _http = http;
        _options = options;
    }

    public async Task SendAsync(TemperatureReading reading, CancellationToken ct)
    {
        var opt = _options.Get("Wis");

        var payload = new
        {
            device = reading.DeviceId,
            tempC = reading.Celsius,
            observedAtUtc = reading.TimestampUtc
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var resp = await _http.PostAsync(opt.IngestPath, content, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }
}
