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
    public class DeleteUserHandler : IRequestHandler<Commands.DeleteUserByParameterCommand, Result<UserRespModel>>
    {
        private readonly IUserService _userService;

        public DeleteUserHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<UserRespModel>> Handle(DeleteUserByParameterCommand request, CancellationToken cancellationToken)
        {
            var data = new UserReqModel { UserId = request.UserId, Username = request.UserName };

            return await _userService.UpdateUserByParameter(data);
        }
    }
}
