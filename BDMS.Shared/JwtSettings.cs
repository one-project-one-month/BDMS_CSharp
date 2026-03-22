using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Shared
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpireMinutes { get; set; }
        public string AdminCookieName { get; set; } = "BDMS_Admin_Token";
        public string ClientCookieName { get; set; } = "BDMS_Client_Token";

    }
}
