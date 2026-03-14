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
    public class CreateUserCommand: IRequest<Result<UserRespModel>>
    {
        public required int UserId { get; set; }
        public int UserRoleId { get; set; }
        public int hospital_id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
