using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class Customer
{
    [JsonPropertyName("customerAccountRelationships")]
    public Relationships Relationships { get; set; } = new();
}