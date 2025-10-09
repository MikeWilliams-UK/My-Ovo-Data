using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class GraphQlRequest
{
    public string OperationName { get; set; }

    public Dictionary<string, object> Variables { get; set; }

    public string Query { get; set; }
}