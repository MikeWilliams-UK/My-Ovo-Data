namespace OvoData.Models.Api.Account;

public class AccountNextV1
{
    public string Id { get; set; } = string.Empty;

    public CustomerAccountRelationshipNextV1 CustomerAccountRelationships { get; set; } = new();
}