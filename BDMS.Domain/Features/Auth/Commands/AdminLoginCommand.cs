using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Commands
{
    public class AdminLoginCommand : IRequest<Result<LoginResultInternal>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
