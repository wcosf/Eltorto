namespace Eltorto.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime CreatedAt { get; set; }
}