using BDMS.Domain.Features.Roles.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Roles.Queries
{
    public class GetAllRoles : IRequest<Result<List<RolesReqRespModel>>>
    {
    }
}
