using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ElectricMeterValue
{
    [JsonPropertyName("registerId")]
    public string RegisterId { get; set; } = string.Empty;

    [JsonPropertyName("timingCategory")]
    public string TimingCategory { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}