namespace NhsWales.TemperatureMonitor.Domain.Models;

/// <summary>
/// Used to represent a temperature reading retrieved temperatures.
/// </summary>
/// <param name="DeviceId">Unique identifier for the device recording temperatures.</param>
/// <param name="Celsius">Temperature received.</param>
/// <param name="TimestampUtc">The date/time of the monitor.</param>
public sealed record TemperatureReading(
    string DeviceId,
    double Celsius,
    DateTimeOffset TimestampUtc)
{
    public static TemperatureReading Create(string deviceId, double celsius, DateTimeOffset timestampUtc)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId is required.", nameof(deviceId));

        if (celsius < -80 || celsius > 150)
            throw new ArgumentOutOfRangeException(nameof(celsius), "Temperature reading is out of expected range.");

        return new TemperatureReading(deviceId.Trim(), celsius, timestampUtc);
    }
}