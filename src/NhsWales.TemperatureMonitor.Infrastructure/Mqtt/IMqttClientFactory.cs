using MQTTnet.Client;

namespace NhsWales.TemperatureMonitor.Infrastructure.Mqtt;

public interface IMqttClientFactory
{
    IMqttClient CreateClient();
}
