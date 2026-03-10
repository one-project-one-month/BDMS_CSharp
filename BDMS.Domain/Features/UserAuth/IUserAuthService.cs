using BDMS.Domain.Features.UserAuth.Commands;
using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.UserAuth
{
    public interface IUserAuthService
    {
        Task<Result<UserLoginResultInternal>> Login(UserLoginCommand request, CancellationToken cancellationToken);
        Task<Result<UserLoginResultInternal>> Register(UserRegisterCommand request, CancellationToken cancellationToken);
    }
}