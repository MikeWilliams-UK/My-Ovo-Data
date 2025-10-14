using System.Collections.Generic;

namespace OvoData.Models.OvoApi.Account;

public class CustomerAccountRelationshipNextV1
{
    public List<CustomerAccountRelationshipEdgeNextV1> Edges { get; set; } = new();
}