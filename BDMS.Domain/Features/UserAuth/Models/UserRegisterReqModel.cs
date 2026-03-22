using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.UserAuth.Models
{
    public class UserRegisterReqModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        // Add other fields if needed for registration, like BloodType, etc. 
        // But the user said "like admin", admin login just has email/password.
        // User registration usually needs username too.
    }
}
