using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class SupplyPoint
{
    public string Sprn { get; set; }

    public string FuelType { get; set; }

    public List<MeterTechnicalDetail> MeterTechnicalDetails { get; set; }

    public Address Address { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}