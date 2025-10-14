using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class Details
{
    [JsonPropertyName("account")]
    public AccountDetail AccountDetail { get; set; } = new();
}