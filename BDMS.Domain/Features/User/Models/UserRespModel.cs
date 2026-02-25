using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Models
{
    public class UserRespModel
    {
        public string UserId { get; set; } = null!;

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? PhoneNo { get; set; }

    }
}


