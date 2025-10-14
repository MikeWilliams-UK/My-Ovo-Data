using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class BootstrapData
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}