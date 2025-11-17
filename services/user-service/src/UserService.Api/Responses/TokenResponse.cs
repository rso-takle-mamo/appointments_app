namespace UserService.Api.Responses;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresIn { get; set; }
}