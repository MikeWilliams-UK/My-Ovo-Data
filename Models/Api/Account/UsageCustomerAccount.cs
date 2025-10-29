using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageCustomerAccount
{
    [JsonPropertyName("node")]
    public UsageDetails Details { get; set; } = new();
}