using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageCustomer
{
    [JsonPropertyName("customerAccountRelationships")]
    public UsageRelationships Relationships { get; set; } = new();
}