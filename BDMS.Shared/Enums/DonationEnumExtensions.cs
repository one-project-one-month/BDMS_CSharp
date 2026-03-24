using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Shared.Enums;

public static class DonationEnumExtensions
{
    public static string ToDatabaseValue(this EnumDonationStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }
}
