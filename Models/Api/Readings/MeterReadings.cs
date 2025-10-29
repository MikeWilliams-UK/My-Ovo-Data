using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadings
{
    [JsonPropertyName("edges")]
    public List<MeterReadingEdge> Edges { get; set; } = new();
}