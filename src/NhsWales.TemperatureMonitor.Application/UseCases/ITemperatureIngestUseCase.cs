using NhsWales.TemperatureMonitor.Domain.Models;

namespace NhsWales.TemperatureMonitor.Application.UseCases;
/// <summary>
/// Used to implement any business logic related to
/// the ingestion of a temperature reading. It basically answers the query of
/// 'What should the system do when a temperature reading arrives?'. It's not so much
/// concerned with how the temperature was received (e.g., MQTT, HTTP, Bluetooth), or where it
/// goes (API, DB etc), just the business workflow. For example, this stage
/// may reject any readings that have an impossible temperatures, any invalid devices, duplicate readings,
/// or alerting thresholds. (Could send an email)
/// </summary>
public interface ITemperatureIngestUseCase
{
    Task ExecuteAsync(TemperatureReading reading, CancellationToken ct);
}
