using Entities.Enums;

namespace Services.Dtos.Registration;

public class RegistrationEmailRequest
{
    public string Email { get; set; } = null!;

    public UserRole Role { get; set; }

    public RegistrationInvitationType Type { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }
}
