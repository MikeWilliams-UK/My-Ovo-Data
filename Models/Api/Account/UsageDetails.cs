using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageDetails
{
    [JsonPropertyName("account")]
    public UsageAccountDetail AccountDetail { get; set; } = new();
}