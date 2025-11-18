namespace UserService.Database.Entities;

public class UserSession
{
    public Guid Id { get; set; }

    public required Guid UserId { get; set; }

    public required string TokenJti { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}