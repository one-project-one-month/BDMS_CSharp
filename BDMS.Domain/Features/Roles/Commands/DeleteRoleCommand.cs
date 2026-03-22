using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles.Commands
{
    public class DeleteRoleCommand: IRequest<Result<RolesReqRespModel>>
    {
        public int Id { get; set; }
    }
}
