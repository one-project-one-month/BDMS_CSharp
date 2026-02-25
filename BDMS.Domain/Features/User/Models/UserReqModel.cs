using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Models
{
    public class UserReqModel
    {
        public string UserId { get; set; } = null!;

        public string? Username { get; set; }
    }
}


