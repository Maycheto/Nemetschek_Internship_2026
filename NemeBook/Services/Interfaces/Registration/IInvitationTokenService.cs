namespace Services.Interfaces.Registration;

public interface IInvitationTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
