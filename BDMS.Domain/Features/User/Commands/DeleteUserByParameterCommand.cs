using BDMS.Domain.Features.User.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Commands
{
    public class DeleteUserByParameterCommand : IRequest<Result<UserRespModel>>
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
