using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;

namespace BDMS.Domain.Features.Auth
{
    public interface IAuthService
    {
        Task<Result<LoginResultInternal>> Login(AdminLoginCommand req, CancellationToken cancellationToken);
    }
}
