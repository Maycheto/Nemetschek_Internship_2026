using Services.Dtos.Registration;

namespace Services.Interfaces.Registration;

public interface IRegistrationEmailSender
{
    Task SendInvitationAsync(RegistrationEmailRequest request, CancellationToken cancellationToken = default);
}
