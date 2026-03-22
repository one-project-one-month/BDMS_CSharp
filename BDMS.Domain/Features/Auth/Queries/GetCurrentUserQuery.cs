using BDMS.Domain.Features.Auth.Models;
using BDMS.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Queries
{
    public class GetCurrentUserQuery : IRequest<Result<LoginResModel>>
    {
        public int UserId { get; set; }
    }
}
