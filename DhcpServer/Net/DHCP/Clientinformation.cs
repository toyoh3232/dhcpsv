using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tohasoft.Net.DHCP.Internal;

namespace Tohasoft.Net.DHCP
{
    public struct ClientInfomation
    {
        public IPAddress RequestAddress;
        public IPAddress ServerAddress;
        public IPAddress ClientAddress;
        public IPAddress YourAddress;
        public IPAddress RelayAgentAddress;
        public string ClientIdentifier;
        public string MacAddress;
        public uint TransactionID;
        public BroadCastType CastType;
    }
}
