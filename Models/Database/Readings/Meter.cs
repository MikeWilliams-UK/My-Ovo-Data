using System.Collections.Generic;

namespace OvoData.Models.Database.Readings;

public class Meter
{
    public string SerialNumber { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<Register> Registers { get; set; } = [];

    public override string ToString()
    {
        if (string.IsNullOrEmpty(SerialNumber))
        {
            return $"{Type} - {Status}";
        }
        else
        {
            return $"{Type} - {SerialNumber} - {Status}";
        }
    }
}