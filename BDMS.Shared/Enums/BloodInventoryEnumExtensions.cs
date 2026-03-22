using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Shared.Enums
{
    public static class BloodInventoryEnumExtensions
    {
        public static string ToDatabaseValue(this EnumBloodInventoryStatus status)
        {
            return status switch
            {
                EnumBloodInventoryStatus.None => "None",
                EnumBloodInventoryStatus.Available => "Available",
                EnumBloodInventoryStatus.Used => "Used",
                EnumBloodInventoryStatus.Expired => "Expired",
                _ => string.Empty
            };
        }

        public static EnumBloodInventoryStatus ToBloodInventoryStatusEnum(this string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return EnumBloodInventoryStatus.None;

            return status.Trim().ToLowerInvariant() switch
            {
                "available" => EnumBloodInventoryStatus.Available,
                "used" => EnumBloodInventoryStatus.Used,
                "expired" => EnumBloodInventoryStatus.Expired,
                _ => EnumBloodInventoryStatus.None
            };
        }
    }
}
