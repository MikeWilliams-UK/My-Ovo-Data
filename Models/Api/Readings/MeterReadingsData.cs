using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class MeterReadingsData
{
    [JsonPropertyName("account")]
    public MeterReadingsAccount Account { get; set; } = new();
}