using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadingsAccount
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accountSupplyPoints")]
    public List<MeterSupplyPoint> MeterSupplyPoints { get; set; } = new();
}