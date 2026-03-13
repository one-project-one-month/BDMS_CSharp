using BDMS.Domain.Features.User.Commands;
using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Handlers
{
    public class CreateUserHandler : IRequestHandler<Commands.CreateUserCommand, Result<UserRespModel>>
    {
        private readonly IUserService _userService;

        public CreateUserHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<UserRespModel>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var data = new UserReqModel { UserId = request.UserId, UserRoleId = request.UserRoleId, Username = request.UserName, Email = request.Email, UserHospitalId = request.hospital_id };

            return await _userService.UpdateUserByParameter(data);
        }
    }
}
