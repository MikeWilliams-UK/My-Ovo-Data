using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class CustomerAccountRelationships
{
    public List<Edge> Edges { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}