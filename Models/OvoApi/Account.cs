using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class Account
{
    public string AccountNo { get; set; }

    public string Id { get; set; }

    public List<AccountSupplyPoint> AccountSupplyPoints { get; set; }

    [JsonPropertyName("__typename")]
    public string __typename { get; set; }
}