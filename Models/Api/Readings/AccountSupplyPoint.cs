using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class AccountSupplyPoint
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("end")]
    public string End { get; set; } = string.Empty;

    [JsonPropertyName("supplyPoint")]
    public SupplyPoint SupplyPoint { get; set; } = new();

    [JsonPropertyName("meterReads_nextV1")]
    public MeterReadings MeterReadings { get; set; } = new();
}