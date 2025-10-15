using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Node
{
    [JsonPropertyName("reading")]
    public Reading Reading { get; set; } = new();
}