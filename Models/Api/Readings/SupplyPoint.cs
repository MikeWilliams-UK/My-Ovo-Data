using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class SupplyPoint
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sprn")]
    public string Sprn { get; set; } = string.Empty;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("fuelType")]
    public string FuelType { get; set; } = string.Empty;

    [JsonPropertyName("meterTechnicalDetails")]
    public List<MeterTechnicalDetail> MeterTechnicalDetails { get; set; } = new();
}