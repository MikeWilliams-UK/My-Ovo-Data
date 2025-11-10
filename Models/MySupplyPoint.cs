using OvoData.Models.Database.Readings;
using System.Collections.Generic;

namespace OvoData.Models;

public class MySupplyPoint
{
    public string Sprn { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;

    public List<SqLiteMeter> Meters { get; set; } = [];
    public List<SqLiteReading> Readings { get; set; } = [];

    public override string ToString()
    {
        return $"{FuelType} - {Sprn}";
    }
}