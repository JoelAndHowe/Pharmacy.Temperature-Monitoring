using NhsWales.TemperatureMonitor.Domain.Models;

namespace NhsWales.TemperatureMonitor.Application.Ports;

/// <summary>
/// In terms of data-flow terminology, a Source produces data, whilst a
/// sink consumes data. Therefore, in this use case, a sink would be used to 
/// process any ingested data, to send it to it's target destination.
/// </summary>
public interface ITemperatureSink
{
    /// <summary>
    /// Called when a temperature is received. 
    /// </summary>
    /// <param name="reading">A record of temperature we've received.</param>
    /// <param name="cancellationToken">A token that indicates if a request has been cancelled.</param>
    /// <returns>A tas that can be run, asynchronously.</returns>
    Task SendAsync(TemperatureReading reading, CancellationToken cancellationToken);
}

/// <summary>
/// Marker interface, indicates a real endpoint sink
/// </summary>
public interface ITemperatureLeafSink : ITemperatureSink { }
