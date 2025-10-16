using System.Collections.Generic;

namespace OvoData.Models;

public class OvoSupplyPoint
{
    public string Sprn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public List<OvoMeter> Meters { get; set; } = [];
    public List<OvoMeterReading> Readings { get; set; } = [];

    public override string ToString()
    {
        return $"{Type} - {Sprn}";
    }
}