using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Data
{
    [JsonPropertyName("account")]
    public Account Account { get; set; } = new();
}