using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class CustomerAccount
{
    [JsonPropertyName("node")]
    public Details Details { get; set; } = new();
}