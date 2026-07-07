using Entities.Models;

namespace Services.Repositories;

public interface IRegistrationInvitationRepository
{
    Task<RegistrationInvitation?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistrationInvitation>> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task CreateAsync(RegistrationInvitation invitation, CancellationToken cancellationToken = default);

    Task UpdateAsync(RegistrationInvitation invitation, CancellationToken cancellationToken = default);
}
