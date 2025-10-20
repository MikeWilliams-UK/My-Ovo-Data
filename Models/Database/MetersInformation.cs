using System;

namespace OvoData.Models.Database;

public class MetersInformation
{
    public string AccountId { get; set; } = string.Empty;

    public DateTime? FirstElectricReading { get; set; }
    public DateTime? LastElectricReading { get; set; }

    public DateTime? FirstGasReading { get; set; }
    public DateTime? LastGasReading { get; set; }
}