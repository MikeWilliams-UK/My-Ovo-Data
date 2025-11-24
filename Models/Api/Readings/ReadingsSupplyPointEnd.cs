using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class ReadingsSupplyPointEnd
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}