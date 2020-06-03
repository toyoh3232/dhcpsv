using Tohasoft.Net.Extension;
using System;
using System.Net;
using System.Text;
using Tohasoft.Net.DHCP.Internal;

namespace Tohasoft.Net.DHCP
{
    class DhcpData
    {

        public bool IsBuiltTobeSent { get; private set; }

        public DhcpServer RelatedServer { get; internal set; }

        private DhcpPacketStruct packet;

        internal DhcpData(byte[] data)
        {
            packet = new DhcpPacketStruct(data);
            IsBuiltTobeSent = false;
        }

        public DhcpMessgeType GetCurrentMessageType()
        {
            
            //　TODO
            if (IsBuiltTobeSent) throw new Exception();
            return packet.options.GetDhcpMessageType();
        }

        internal byte[] BuildSendData(DhcpMessgeType msgType, IPAddress clientIp)
        {
            if (!IsBuiltTobeSent)
            {
                packet.ApplySettings(msgType, RelatedServer._settings, clientIp);
                IsBuiltTobeSent = true;
                return packet.ToArray();
            }
            throw new Exception("Dhcp packet data is already built.");

        }
        


        public ClientInfomation GetClientInfo()
        {
            // TODO
            if (IsBuiltTobeSent) throw new Exception();

            var client = new ClientInfomation
            {
                MacAddress = packet.chaddr.ToString(packet.hlen),
                ClientAddress = new IPAddress(packet.ciaddr),
                YourAddress = new IPAddress(packet.yiaddr),
                TransactionID = BitConverter.ToUInt32(packet.xid, 0),
                RelayAgentAddress = new IPAddress(packet.giaddr),
                CastType = (BroadCastType)packet.flags[0]
                
            };
            
            if (packet.options.GetOptionData(DhcpOptionType.ClientIdentifier) != null)
                client.ClientIdentifier = Encoding.Default.GetString(packet.options.GetOptionData(DhcpOptionType.ClientIdentifier));
            if (packet.options.GetOptionData(DhcpOptionType.RequestedIPAddress) != null)
                client.RequestAddress = new IPAddress(packet.options.GetOptionData(DhcpOptionType.RequestedIPAddress));
            if (packet.options.GetOptionData(DhcpOptionType.ServerIdentifier) != null)
                client.ServerAddress = new IPAddress(packet.options.GetOptionData(DhcpOptionType.ServerIdentifier));

            return client;
        }
    }
}
