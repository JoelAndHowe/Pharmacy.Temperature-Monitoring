using MQTTnet;
using MQTTnet.Client;
using NhsWales.TemperatureMonitor.Infrastructure.Mqtt;

public sealed class MqttNetClientFactory : IMqttClientFactory
{
    public IMqttClient CreateClient()
    {
        return new MQTTnet.MqttFactory().CreateMqttClient();
    }
}
