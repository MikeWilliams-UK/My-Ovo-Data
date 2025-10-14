using System.Collections.Generic;

namespace OvoData.Models.Api.Account;

public class CustomerAccountRelationshipNextV1
{
    public List<CustomerAccountRelationshipEdgeNextV1> Edges { get; set; } = new();
}