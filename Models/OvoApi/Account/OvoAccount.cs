namespace OvoData.Models.OvoApi.Account;

public class OvoAccount
{
    public string Id { get; set; } = string.Empty;

    public bool HasElectric { get; set; }

    public bool HasGas { get; set; }

    public override string ToString()
    {
        return Id;
    }
}