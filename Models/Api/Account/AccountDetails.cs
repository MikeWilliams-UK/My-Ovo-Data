using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class AccountDetails
{
    [JsonPropertyName("account")]
    public AccountDetail AccountDetail { get; set; } = new();
}