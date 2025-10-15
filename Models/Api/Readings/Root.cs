using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Root
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}