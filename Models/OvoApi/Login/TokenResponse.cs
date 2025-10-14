namespace OvoData.Models.OvoApi.Login;

public class TokenResponse
{
    public AccessToken AccessToken { get; set; } = new();
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
}