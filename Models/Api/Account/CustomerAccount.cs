using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class CustomerAccount
{
    [JsonPropertyName("node")]
    public AccountDetails AccountDetails { get; set; } = new();
}