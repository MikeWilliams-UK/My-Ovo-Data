using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class AccountsResponse
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}