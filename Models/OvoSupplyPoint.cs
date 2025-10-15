using System.Collections.Generic;

namespace OvoData.Models;

public class OvoSupplyPoint
{
    public List<OvoMeter> ElectricMeters { get; set; } = [];
    public List<OvoReading> ElectricReadings { get; set; } = [];

    public List<OvoMeter> GasMeters { get; set; } = [];
    public List<OvoReading> GasReadings { get; set; } = [];
}