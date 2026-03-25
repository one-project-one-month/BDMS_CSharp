using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Domain.Features.User.Models
{
    public class UserRespModel
    {
        public int UserId { get; set; }
        //public int UserRoleId { get; set; }
        //public int? UserHospitalId { get; set; }
        public RoleModel? Role { get; set; }
        public HospitalModel? Hospital { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleModel
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
    }

    public class HospitalModel
    {
        public int? HospitalId { get; set; }
        public string? HospitalName { get; set; }
      
    }

}


