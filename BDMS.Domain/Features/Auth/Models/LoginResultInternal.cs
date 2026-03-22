using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.Auth.Models
{
    public class LoginResultInternal
    {
        public LoginResModel UserInfo { get;set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpireToken { get; set; }
    }
}
