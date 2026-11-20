using LifeLink.Application.Commands.Requesters;

namespace LifeLink.Application.Interfaces.Services;

public interface IRequesterService
{
    Task<int> RegisterRequesterAsync(
        RegisterRequesterCommand command,
        CancellationToken cancellationToken = default);
}