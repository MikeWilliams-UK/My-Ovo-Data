using System;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Register
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
    public DateTime RegisterStartDate { get; set; } = DateTime.MinValue;

    [JsonPropertyName("registerEndDate")]
    public DateTime? RegisterEndDate { get; set; } = null;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}