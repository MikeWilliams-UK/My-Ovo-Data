using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class SupplyPoint
{
    public string Sprn { get; set; }

    public string FuelType { get; set; }

    public List<MeterTechnicalDetail> MeterTechnicalDetails { get; set; }

    public Address Address { get; set; }
}