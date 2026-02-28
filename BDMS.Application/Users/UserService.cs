using BDMS.Domain.Entities;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Application.Users;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);
    }
}
