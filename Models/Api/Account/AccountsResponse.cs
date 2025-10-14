using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class AccountsResponse
{
    [JsonPropertyName("data")]
    public AccountsData Data { get; set; } = new();
}