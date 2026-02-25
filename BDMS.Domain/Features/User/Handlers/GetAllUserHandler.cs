using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Domain.Features.User.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Handlers;

public class GetAllUserHandler : IRequestHandler<Queries.GetAllUserQuery, Result<List<UserRespModel>>>
{
    private readonly AppDbContext _appDbContext;

    public GetAllUserHandler(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Result<List<UserRespModel>>> Handle(Queries.GetAllUserQuery request, CancellationToken cancellationToken)
    {
        var service = new UserService(_appDbContext);
        return await service.GetAllUser();
    }
}
