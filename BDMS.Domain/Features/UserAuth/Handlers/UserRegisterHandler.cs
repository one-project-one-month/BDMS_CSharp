using BDMS.Domain.Features.UserAuth.Commands;
using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.UserAuth.Handlers
{
    public class UserRegisterHandler : IRequestHandler<UserRegisterCommand, Result<UserLoginResultInternal>>
    {
        private readonly UserAuthService _userAuthService;

        public UserRegisterHandler(UserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }

        public async Task<Result<UserLoginResultInternal>> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            return await _userAuthService.Register(request, cancellationToken);
        }
    }
}
