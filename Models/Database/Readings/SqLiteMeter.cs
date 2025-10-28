using System.Collections.Generic;

namespace OvoData.Models.Database.Readings;

public class SqLiteMeter
{
    public string SerialNumber { get; set; } = string.Empty;

    public string FuelType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<SqLiteRegister> Registers { get; set; } = [];

    public override string ToString()
    {
        return string.IsNullOrEmpty(SerialNumber)
            ? $"{FuelType} - {Status}"
            : $"{FuelType} - {SerialNumber} - {Status}";
    }
}