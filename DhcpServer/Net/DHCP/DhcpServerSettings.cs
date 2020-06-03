using System.Net;
using System.Security.Policy;
using Tohasoft.Net.Extension;

namespace Tohasoft.Net.DHCP
{
    public struct DhcpServerSettings
    {
        public IPAddress ServerIp;

        public IPAddress StartIp;
        public IPAddress EndIp;
        
        public IPAddress SubnetMask;
        public IPAddress RouterIp;
        public IPAddress DomainIp;

        public uint LeaseTime;
        public string ServerName;

        public bool IsValid() => ServerIp != null && ((StartIp == null && EndIp == null) || (StartIp != null && EndIp != null));

    }
}