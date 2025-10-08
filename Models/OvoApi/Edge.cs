using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class Edge
{
    public Node Node { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}