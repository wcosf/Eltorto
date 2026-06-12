using Microsoft.AspNetCore.Identity;

namespace Eltorto.Domain.Entities;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
