using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Services.Repositories;

namespace Data.Repositories;

public class RegistrationInvitationRepository : IRegistrationInvitationRepository
{
    private readonly NemeBookDbContext dbContext;

    public RegistrationInvitationRepository(NemeBookDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<RegistrationInvitation?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RegistrationInvitations
            .Include(invitation => invitation.User)
            .Include(invitation => invitation.Students)
            .ThenInclude(student => student.User)
            .FirstOrDefaultAsync(invitation => invitation.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<RegistrationInvitation>> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RegistrationInvitations
            .Include(invitation => invitation.User)
            .Include(invitation => invitation.Students)
            .ThenInclude(student => student.User)
            .Where(invitation =>
                invitation.Email == email &&
                invitation.UsedAtUtc == null &&
                invitation.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(
        RegistrationInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RegistrationInvitations.AddAsync(invitation, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        RegistrationInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        dbContext.RegistrationInvitations.Update(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
