namespace NhsWales.TemperatureMonitor.Infrastructure.Options;

public sealed record MqttOptions(string Host, int Port, string Topic);
