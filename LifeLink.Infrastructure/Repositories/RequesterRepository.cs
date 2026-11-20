using LifeLink.Application.Exceptions;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Repositories;

public class RequesterRepository : IRequesterRepository
{
    private readonly LifeLinkDbContext _context;

    public RequesterRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task<int> RegisterAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var userEntity = new Data.Entities.User
        {
            FullName = user.FullName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        await _context.Users.AddAsync(
            userEntity,
            cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            user.Id = userEntity.Id;

            return userEntity.Id;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 || sqlException.Number == 2627) &&
                  sqlException.Message.Contains(
                      "UQ_Users_Email",
                      StringComparison.OrdinalIgnoreCase))
        {
            throw new DuplicateEmailException(user.Email);
        }
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(
                u => u.Email == email,
                cancellationToken);
    }

    public async Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AnyAsync(
                c => c.Id == cityId,
                cancellationToken);
    }
}