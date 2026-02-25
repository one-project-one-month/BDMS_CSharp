using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Handlers;

public class UpdateUserHandler : IRequestHandler<Commands.UpdateUserCommand, Result<UserRespModel>>
{
    private readonly AppDbContext _appDbContext;

    public UpdateUserHandler(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Result<UserRespModel>> Handle(Commands.UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var service = new UserService(_appDbContext);
        return await service.UpdateUser(request.UserId, request.PhoneNo);
    }
}
