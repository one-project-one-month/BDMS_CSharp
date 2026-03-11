using BDMS.Domain.Features.UserAuth.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.UserAuth.Commands
{
    public class UserLoginCommand : IRequest<Result<UserLoginResultInternal>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
