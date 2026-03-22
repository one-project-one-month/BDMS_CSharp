using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Models
{
    public class UserReqModel
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int UserRoleId { get; set; }
        public int UserHospitalId { get; set; }
        public string? Email { get; set; }
    }
}


