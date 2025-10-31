using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ReadingsResponse
{
    [JsonPropertyName("data")]
    public ReadingsData Data { get; set; } = new();
}