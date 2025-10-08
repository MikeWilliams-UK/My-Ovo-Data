namespace OvoData.Models.OvoApi;

public class OvoAccount
{
    public string Id { get; set; }

    public bool HasElectric { get; set; }

    public bool HasGas { get; set; }

    public override string ToString()
    {
        return Id;
    }
}