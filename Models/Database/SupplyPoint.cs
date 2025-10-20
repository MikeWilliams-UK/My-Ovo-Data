using OvoData.Models.Database.Readings;
using System.Collections.Generic;

namespace OvoData.Models.Database;

public class SupplyPoint
{
    public string Sprn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public List<Meter> Meters { get; set; } = [];
    public List<Reading> Readings { get; set; } = [];

    public override string ToString()
    {
        return $"{Type} - {Sprn}";
    }
}