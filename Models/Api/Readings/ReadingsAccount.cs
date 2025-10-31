using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ReadingsAccount
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accountSupplyPoints")]
    public List<ReadingsAccountSupplyPoint> MeterSupplyPoints { get; set; } = new();
}