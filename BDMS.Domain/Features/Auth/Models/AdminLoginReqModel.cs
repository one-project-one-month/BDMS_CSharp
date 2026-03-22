using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Models
{
    public class AdminLoginReqModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
