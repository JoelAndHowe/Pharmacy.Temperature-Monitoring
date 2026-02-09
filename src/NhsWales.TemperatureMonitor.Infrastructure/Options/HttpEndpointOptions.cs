namespace NhsWales.TemperatureMonitor.Infrastructure.Options;

public sealed class HttpEndpointOptions
{
    public string BaseUrl { get; set; } = "";
    public string IngestPath { get; set; } = "";
    public string Token { get; set; } = "";
}
