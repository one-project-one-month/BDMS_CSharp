using BDMS.Domain.Features.Auth.Commands;
using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth
{
    public interface IAuthService
    {
        Task<Result<LoginResultInternal>> Login(AdminLoginCommand req, CancellationToken cancellationToken);
    }
}
