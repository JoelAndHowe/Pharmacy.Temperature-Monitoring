using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

var host = "localhost";
var port = 1883;
var topic = "poc/device1/temperature";

var factory = new MqttFactory();
var client = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer(host, port)
    .WithCleanSession()
    .Build();

await client.ConnectAsync(options);

Console.WriteLine($"Connected. Publishing to '{topic}'. Enter a temp (e.g. 4.2) or blank to auto.");

var rng = new Random();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    double temp;
    if (string.IsNullOrWhiteSpace(input))
        temp = Math.Round(2 + rng.NextDouble() * 10, 2);
    else if (!double.TryParse(input, out temp))
    {
        Console.WriteLine("Not a number.");
        continue;
    }

    var payload = JsonSerializer.Serialize(new
    {
        deviceId = "device1",
        celsius = temp,
        timestampUtc = DateTimeOffset.UtcNow
    });

    var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(Encoding.UTF8.GetBytes(payload))
        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    await client.PublishAsync(message);

    Console.WriteLine($"Published: {payload}");
}
