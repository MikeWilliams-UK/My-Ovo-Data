using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageRelationships
{
    [JsonPropertyName("edges")]
    public List<UsageCustomerAccount> Accounts { get; set; } = new();
}