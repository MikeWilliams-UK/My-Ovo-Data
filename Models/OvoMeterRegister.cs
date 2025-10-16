using System;

namespace OvoData.Models;

public class OvoMeterRegister
{
    public string Id { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string TimingCategory { get; set; } = string.Empty;
    public string UnitOfMeasurement { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{StartDate} - {EndDate} - {TimingCategory} - {UnitOfMeasurement}";
    }
}