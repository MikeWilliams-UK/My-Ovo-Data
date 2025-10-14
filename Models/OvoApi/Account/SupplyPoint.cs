using System.Collections.Generic;

namespace OvoData.Models.OvoApi.Account;

public class SupplyPoint
{
    public string Sprn { get; set; } = string.Empty;

    public string FuelType { get; set; } = string.Empty;

    public List<SupplyPointMeterTechnicalDetails> MeterTechnicalDetails { get; set; } = new();

    public SupplyPointAddress Address { get; set; } = new();
}