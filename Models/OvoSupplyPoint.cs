using System.Collections.Generic;

namespace OvoData.Models;

public class OvoSupplyPoint
{
    public string Sprn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public List<OvoMeter> ElectricMeters { get; set; } = [];
    public List<OvoMeterReading> ElectricReadings { get; set; } = [];

    public List<OvoMeter> GasMeters { get; set; } = [];
    public List<OvoMeterReading> GasReadings { get; set; } = [];
}