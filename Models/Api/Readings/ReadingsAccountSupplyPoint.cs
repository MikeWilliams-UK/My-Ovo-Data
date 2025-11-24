using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ReadingsAccountSupplyPoint
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("end")]
    public ReadingsSupplyPointEnd Ending { get; set; } = new();

    [JsonPropertyName("supplyPoint")]
    public ReadingsSupplyPoint SupplyPoint { get; set; } = new();

    [JsonPropertyName("meterReads_nextV1")]
    public Readings Readings { get; set; } = new();
}