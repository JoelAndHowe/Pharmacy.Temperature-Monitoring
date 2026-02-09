using NhsWales.TemperatureMonitor.Domain.Models;

namespace NhsWales.TemperatureMonitor.Application.Ports;

/// <summary>
/// Fans out by sending updates to all <see cref="ITemperatureLeafSink"/> instances that have been registed
/// in the DI.
/// </summary>
public sealed class FanOutTemperatureSink : ITemperatureSink
{
    private readonly IReadOnlyList<ITemperatureLeafSink> _leafSinks;

    /// <summary>
    /// Constructor that sets injected leaf sinks.
    /// </summary>
    /// <param name="leafSinks">Collection of leaf sinks</param>
    public FanOutTemperatureSink(IEnumerable<ITemperatureLeafSink> leafSinks)
        => _leafSinks = leafSinks.ToList();

    // <inheritdoc />
    public async Task SendAsync(TemperatureReading reading, CancellationToken cancellationToken)
    {
        // Best-effort: attempt all sinks, then fail with aggregate if any failed.
        // If you want “never throw”, remove the aggregate throw section.
        List<Exception>? failures = null;

        foreach (var sink in _leafSinks)
        {
            try
            {
                await sink.SendAsync(reading, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                failures ??= new List<Exception>();
                failures.Add(ex);
            }
        }

        if (failures is { Count: > 0 })
            throw new AggregateException("One or more temperature sinks failed.", failures);
    }
}
