using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class Node
{
    public Account Account { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}