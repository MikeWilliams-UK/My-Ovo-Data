using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ReadingsData
{
    [JsonPropertyName("account")]
    public ReadingsAccount Account { get; set; } = new();
}