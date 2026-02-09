using NhsWales.TemperatureMonitor.Application.Ports;
using NhsWales.TemperatureMonitor.Application.UseCases;
using NhsWales.TemperatureMonitor.Domain.Models;

public sealed class ReportTemperaturesToInterfacesUseCase : ITemperatureIngestUseCase
{
    private readonly ITemperatureSink _sink;

    public ReportTemperaturesToInterfacesUseCase(ITemperatureSink sink) => _sink = sink;

    public Task ExecuteAsync(TemperatureReading reading, CancellationToken ct)
        => _sink.SendAsync(reading, ct);
}
