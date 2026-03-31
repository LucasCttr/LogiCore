using System.ComponentModel.DataAnnotations;

namespace LogiCore.Domain.Entities;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    // FK to Identity user
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
