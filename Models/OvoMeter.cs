using System.Collections.Generic;

namespace OvoData.Models;

public class OvoMeter
{
    public string SerialNumber { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<OvoMeterRegister> Registers { get; set; } = [];

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