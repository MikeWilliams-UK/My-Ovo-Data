using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; } = new();
}