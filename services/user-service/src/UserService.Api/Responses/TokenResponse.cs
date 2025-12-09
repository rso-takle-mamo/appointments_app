namespace UserService.Api.Responses;

public class TokenResponse
{
    /// <summary>
    /// The JWT access token
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The expiration date and time of the token
    /// </summary>
    /// <example>2025-12-09T12:00:00Z</example>
    public DateTime ExpiresIn { get; set; }
}