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
    public class UserStatusHandler : IRequestHandler<UserStatusCommand, Result<UserRespModel>>
    {
        private readonly IUserService _userService;
        public UserStatusHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<Result<UserRespModel>> Handle(UserStatusCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateUserStatus(request.UserId, request.IsActive);
        }
    }
}
