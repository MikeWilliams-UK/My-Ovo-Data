using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterTechnicalDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("meterSerialNumber")]
    public string MeterSerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("registers")]
    public List<MeterRegister> MeterRegisters { get; set; } = [];
}