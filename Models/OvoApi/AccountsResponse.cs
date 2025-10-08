using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class AccountsResponse
{
    [JsonPropertyName("customer_nextV1")]
    public CustomerNextV1 CustomerNextV1 { get; set; }
}