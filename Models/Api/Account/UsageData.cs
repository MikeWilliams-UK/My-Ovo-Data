using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageData
{
    [JsonPropertyName("customer_nextV1")]
    public UsageCustomer Customer { get; set; } = new();
}