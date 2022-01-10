using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SSSpy.Core.Attributes;

namespace SSSpy.MsSQL
{
    public enum MsSQLNetworkLibrary
    {
        [TextValue("dbnmpntw")]
        NamedPipes,
        [TextValue("dbmsrpcn")]
        MultiprotocolRPC,
        [TextValue("dbmsvinn")]
        BanyanVines,
        [TextValue("dbmsspxn")]
        IPXSPX,
        [TextValue("dbmssocn")]
        TCPIP,
    }

    public static class MsSQLNetworkLibraryExtends
    {
        public static string ToText(this MsSQLNetworkLibrary i)
        {
            foreach (object ca in i.GetType().GetCustomAttributes(true))
            {
                if (ca is TextValueAttribute)
                {
                    return (ca as TextValueAttribute).Value;
                }
            }
            return i.ToString();
        }
    }
}
