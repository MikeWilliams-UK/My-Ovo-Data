using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageResponse
{
    [JsonPropertyName("data")]
    public UsageData Data { get; set; } = new();
}