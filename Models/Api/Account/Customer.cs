namespace OvoData.Models.Api.Account;

public class Customer
{
    public string Id { get; set; } = string.Empty;

    public CustomerAccounts CustomerAccountRelationships { get; set; } = new();
}