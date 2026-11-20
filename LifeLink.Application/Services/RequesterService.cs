using LifeLink.Application.Commands.Requesters;
using LifeLink.Application.Exceptions;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Domain.Entities;

namespace LifeLink.Application.Services;

public class RequesterService : IRequesterService
{
    private readonly IRequesterRepository _requesterRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RequesterService(
        IRequesterRepository requesterRepository,
        IPasswordHasher passwordHasher)
    {
        _requesterRepository = requesterRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> RegisterRequesterAsync(
        RegisterRequesterCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate City
        var cityExists = await _requesterRepository.CityExistsAsync(
            command.CityId,
            cancellationToken);

        if (!cityExists)
        {
            throw new CityNotFoundException(command.CityId);
        }

        // 2. Normalize Email
        var email = command.Email.Trim().ToLowerInvariant();

        // 3. Check Duplicate Email
        var emailExists = await _requesterRepository.EmailExistsAsync(
            email,
            cancellationToken);

        if (emailExists)
        {
            throw new DuplicateEmailException(email);
        }

        // 4. Hash Password
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        // 5. Create User
        var user = new User
        {
            FullName = command.FullName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            PhoneNumber = command.PhoneNumber.Trim(),
            Role = "Requester",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 6. Save User
        return await _requesterRepository.RegisterAsync(
            user,
            cancellationToken);
    }
}