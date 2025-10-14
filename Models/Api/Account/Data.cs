using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class Data
{
    [JsonPropertyName("customer_nextV1")]
    public Customer Customer { get; set; } = new();
}