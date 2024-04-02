using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class AccountsResponse
{
    public string CustomerId { get; set; }
    public bool IsFirstLogin { get; set; }
    public List<string> AccountIds { get; set; }
}