using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterRegister
{
    [JsonPropertyName("registerId")]
    public string RegisterId { get; set; } = string.Empty;

    [JsonPropertyName("timingCategory")]
    public string TimingCategory { get; set; } = string.Empty;

    [JsonPropertyName("numberOfDigits")]
    public string NumberOfDigits { get; set; } = string.Empty;

    [JsonPropertyName("unitMeasurement")]
    public string UnitMeasurement { get; set; } = string.Empty;

    [JsonPropertyName("registerStartDate")]
    public string RegisterStartDate { get; set; } = string.Empty;

    [JsonPropertyName("registerEndDate")]
    public string RegisterEndDate { get; set; } = string.Empty;
}