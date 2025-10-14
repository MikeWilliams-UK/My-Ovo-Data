using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class CustomerAccounts
{
    [JsonPropertyName("edges")]
    public List<CustomerAccount> Accounts { get; set; } = new();
}