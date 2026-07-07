using Entities.Enums;

namespace Entities.Models;

public class RegistrationInvitation
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public UserRole Role { get; set; }

    public RegistrationInvitationType Type { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public List<Student> Students { get; set; } = new List<Student>();
}
