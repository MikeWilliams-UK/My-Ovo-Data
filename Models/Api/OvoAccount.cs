namespace OvoData.Models.Api;

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