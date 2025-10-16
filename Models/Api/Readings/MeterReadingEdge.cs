using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadingEdge
{
    [JsonPropertyName("node")]
    public MeterReadingNode MeterNode { get; set; } = new();
}