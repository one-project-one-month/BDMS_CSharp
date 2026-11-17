using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BDMS.Domain.Features.Auth.Models;

namespace BDMS.Domain.Features.UserAuth.Models
{
    public class UserAuthResModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();
        public CurrentUserDonorResModel? Donor { get; set; }
    }

    public class UserLoginResultInternal
    {
        public UserAuthResModel UserInfo { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpireToken { get; set; }
    }
}
