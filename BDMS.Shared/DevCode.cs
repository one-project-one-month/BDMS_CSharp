using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDMS.Shared
{
    public static class DevCode
    {
        public static string? ToJson(this object? obj, bool format = false)
        {
            if (obj == null) return string.Empty;
            string? result;
            if (obj is string)
            {
                result = obj.ToString();
                goto Result;
            }

            var settings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTHH:mm:ss.sssZ" };
            result = format
                ? JsonConvert.SerializeObject(obj, Formatting.Indented, settings)
                : JsonConvert.SerializeObject(obj, settings);
        Result:
            return result;
        }

        public static T ToObject<T>(this string jsonStr)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(jsonStr,
                    new JsonSerializerSettings { DateParseHandling = DateParseHandling.DateTimeOffset });
                return result!;
            }
            catch
            {
                return (T)Convert.ChangeType(jsonStr, typeof(T));
            }
        }
    }
}
