using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadingData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("lifecycle")]
    public string Lifecycle { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("meterSerialNumber")]
    public string MeterSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// If meter type is GAS then value lives here
    /// </summary>
    [JsonPropertyName("value")]
    public string GasMeterValue { get; set; } = string.Empty;

    /// <summary>
    /// If meter type is ELECTRIC then value lives in the first electric meter value
    /// </summary>
    [JsonPropertyName("registers")]
    public List<ElectricMeterValue> ElectricMeterValues { get; set; } = [];
}