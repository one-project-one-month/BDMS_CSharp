using BDMS.Domain.Entities;

namespace BDMS.Application.Users;

public interface IUserService
{
    Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
}
