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
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<UserLoginResultInternal>>
    {
        private readonly IUserAuthService _userAuthService;
        public RefreshTokenHandler(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }
        public async Task<Result<UserLoginResultInternal>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _userAuthService.RefreshToken(request.UserId, cancellationToken);
        }
    }
}
