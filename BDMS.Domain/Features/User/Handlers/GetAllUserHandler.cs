using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Domain.Features.User.Queries;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Handlers;

public class GetAllUserHandler : IRequestHandler<Queries.GetAllUserQuery, Result<List<UserRespModel>>>
{
    private readonly IUserService _userService;

    public GetAllUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<List<UserRespModel>>> Handle(Queries.GetAllUserQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetAllUser();
    }
}
