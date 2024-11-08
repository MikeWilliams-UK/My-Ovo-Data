using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class AccountsResponse
{
    public string CustomerId { get; set; } = string.Empty;
    public bool IsFirstLogin { get; set; }
    public List<string> AccountIds { get; set; } = new();
}