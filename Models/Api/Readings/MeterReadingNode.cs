using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadingNode
{
    [JsonPropertyName("reading")]
    public MeterReadingData MeterReadingData { get; set; } = new();
}