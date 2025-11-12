using System;

namespace OvoData.Models.Export;

public class RegistersData
{
    public DateTime StartDate { get; set; } = DateTime.MinValue;
    public DateTime EndDate { get; set; } = DateTime.MaxValue;

    public string FuelType { get; set; } = string.Empty;
    public string TimingCategory { get; set; } = string.Empty;
    public string UnitOfMeasurement { get; set; } = string.Empty;
}