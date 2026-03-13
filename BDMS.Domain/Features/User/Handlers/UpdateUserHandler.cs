using BDMS.Database.AppDbContextModels;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;

namespace BDMS.Domain.Features.User.Handlers;

public class UpdateUserHandler : IRequestHandler<Commands.UpdateUserCommand, Result<UserRespModel>>
{
    private readonly IUserService _userService;

    public UpdateUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<UserRespModel>> Handle(Commands.UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var data = new UserReqModel{UserId = request.UserId, UserRoleId = request.UserRoleId, Username = request.UserName, Email = request.Email, UserHospitalId = request.hospital_id };

        return await _userService.UpdateUserByParameter(data);
    }
}
