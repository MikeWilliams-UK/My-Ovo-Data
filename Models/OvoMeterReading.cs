using System;

namespace OvoData.Models;

public class OvoMeterReading
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string LifeCycle { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string MeterSerialNumber { get; set; } = string.Empty;

    public string RegisterId { get; set; } = string.Empty;
    public string TimingCategory { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd} - {MeterSerialNumber} - {Source} - {Value}";
    }
}