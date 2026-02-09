using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using NhsWales.TemperatureMonitor.Application.Ports;
using NhsWales.TemperatureMonitor.Application.UseCases;
using NhsWales.TemperatureMonitor.Infrastructure.Http;
using NhsWales.TemperatureMonitor.Infrastructure.Mqtt;
using NhsWales.TemperatureMonitor.Infrastructure.Options;
using NhsWales.TemperatureMonitor.Worker;

var builder = Host.CreateApplicationBuilder(args);

// ---- Bind MQTT options ----
var mqttHost = builder.Configuration["Mqtt:Host"] ?? "localhost";
var mqttPort = int.TryParse(builder.Configuration["Mqtt:Port"], out var p) ? p : 1883;
var mqttTopic = builder.Configuration["Mqtt:Topic"] ?? "poc/device1/temperature";

builder.Services.AddSingleton(new MqttOptions(mqttHost, mqttPort, mqttTopic));
builder.Services.AddSingleton<MqttTemperatureSubscriber>();

// ---- Named options for endpoints (Primary/Wis/...) ----
builder.Services.AddOptions<HttpEndpointOptions>("Primary")
    .Bind(builder.Configuration.GetSection("HttpEndpoints:Primary"))
    .Validate(o => Uri.IsWellFormedUriString(o.BaseUrl, UriKind.Absolute), "Primary BaseUrl invalid")
    .Validate(o => !string.IsNullOrWhiteSpace(o.IngestPath), "Primary IngestPath missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Token), "Primary Token missing (use user-secrets/env var)")
    .ValidateOnStart();

builder.Services.AddOptions<HttpEndpointOptions>("Wis")
    .Bind(builder.Configuration.GetSection("HttpEndpoints:Wis"))
    .Validate(o => Uri.IsWellFormedUriString(o.BaseUrl, UriKind.Absolute), "Wis BaseUrl invalid")
    .Validate(o => !string.IsNullOrWhiteSpace(o.IngestPath), "Wis IngestPath missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Token), "Wis Token missing (use user-secrets/env var)")
    .ValidateOnStart();


builder.Services.AddHttpClient<WisTemperatureSink>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptionsMonitor<HttpEndpointOptions>>().Get("Wis");
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.Token);
});

// ---- Register Leaf sinks (multiple registrations) ----
builder.Services.AddSingleton<ITemperatureLeafSink>(sp => sp.GetRequiredService<WisTemperatureSink>());

// ---- Register Composite sink used by use case ----
builder.Services.AddSingleton<ITemperatureSink, FanOutTemperatureSink>();

builder.Services.AddSingleton<ITemperatureIngestUseCase, ReportTemperaturesToInterfacesUseCase>();

// ---- Worker host ----
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IMqttClientFactory,
                              MqttNetClientFactory>();

await builder.Build().RunAsync();
