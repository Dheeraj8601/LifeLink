using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly LifeLinkDbContext _context;

    public AuthRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email,CancellationToken cancellationToken = default)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email == email,
                cancellationToken);

        if (userEntity is null)
            return null;

        return new User
        {
            Id = userEntity.Id,
            FullName = userEntity.FullName,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            PhoneNumber = userEntity.PhoneNumber,
            Role = userEntity.Role,
            IsActive = userEntity.IsActive,
            CreatedAt = userEntity.CreatedAt
        };
    }
}