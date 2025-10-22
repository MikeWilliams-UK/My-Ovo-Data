namespace OvoData.Models;

public class Account
{
    public string Id { get; set; } = string.Empty;

    public bool HasElectric { get; set; }

    public string ElectricStartDate { get; set; } = string.Empty;

    public bool HasGas { get; set; }

    public string GasStartDate { get; set; } = string.Empty;

    public override string ToString()
    {
        return Id;
    }
}