namespace OvoData.Models.OvoApi;

public class TokenResponse
{
    public AccessToken AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
}