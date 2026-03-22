using BDMS.Domain.Features.Permissions.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Permissions.Commands
{
    public class DeletePermissionCommand: IRequest<Result<PermissionReqRespModel>>
    {
        public int Id { get; set; }
    }
}
