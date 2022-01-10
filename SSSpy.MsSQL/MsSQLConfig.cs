using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace SSSpy.MsSQL
{
    public class MsSQLConfig
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool? TrustedConnection { get; set; } // 是否使用 Windows 用户链接。
        public int? ConnectTimeout { get; set; } // 秒
        public bool? Pooling { get; set; } // 默认 真，是否使用连接池。
        public int? MaxPoolSize { get; set; }
        public int? MinPoolSize { get; set; }
        public MsSQLNetworkLibrary? NetworkLibrary { get; set; }

        public override string ToString()
        {
            Regex re = new Regex(@"[A-Z]");
            List<string> configs = new List<string>();
            foreach(PropertyInfo pi in GetType().GetProperties())
            {
                object v = pi.GetValue(this, null);
                if (v != null)
                {
                    string n = re.Replace(pi.Name, m => " " + m.Value).Trim();
                    if (v is MsSQLNetworkLibrary)
                    {
                        string ev = ((MsSQLNetworkLibrary)v).ToText();
                        configs.Add($"{n}={ev}");
                    } else
                    {
                        configs.Add($"{n}={v}");
                    }
                }
            }
            return string.Join(";", configs.ToArray());
        }
    }
}
