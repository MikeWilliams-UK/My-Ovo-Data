using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi.Account;

public class Data
{
    [JsonPropertyName("customer_nextV1")]
    public AccountNextV1 CustomerNextV1 { get; set; } = new();
}