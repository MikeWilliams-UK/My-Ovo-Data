using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadsNextV1
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; } = new();
}