using Tohasoft.Net.Extension;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Tohasoft.Net.DHCP.Internal
{
    internal struct DhcpPacketStruct
    {
        internal const int OptionOffset = 240;

        public DhcpPacketStruct(byte[] data) : this()
        {
            try
            {
                using (var stream = new MemoryStream(data, 0, data.Length))
                using (var reader = new BinaryReader(stream))
                {
                    op = reader.ReadByte();
                    htype = reader.ReadByte();
                    hlen = reader.ReadByte();
                    hops = reader.ReadByte();
                    xid = reader.ReadBytes(4);
                    secs = reader.ReadBytes(2);
                    flags = reader.ReadBytes(2);
                    ciaddr = reader.ReadBytes(4);
                    yiaddr = reader.ReadBytes(4);
                    siaddr = reader.ReadBytes(4);
                    giaddr = reader.ReadBytes(4);
                    chaddr = reader.ReadBytes(16);
                    sname = reader.ReadBytes(64);
                    file = reader.ReadBytes(128);
                    cookie = reader.ReadBytes(4);
                    options = new Options(reader.ReadBytes(data.Length - OptionOffset));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"{this.GetType().FullName}:{e.Message}");
            }

        }

        public void ApplySettings(DhcpMessgeType msgType, DhcpServerSettings server, IPAddress clientIp)
        {
           
            switch (msgType)
            {
                case DhcpMessgeType.DHCP_OFFER:
                    op = (byte)BootMessageType.BootReply;
                    // htype
                    // hlen
                    hops = 0;
                    // xid from client DHCPDISCOVER message
                    secs.FillZero();
                    ciaddr.FillZero();
                    yiaddr = IPAddress.Parse(clientIp.ToString()).GetAddressBytes();
                    siaddr = IPAddress.Parse("0.0.0.0").GetAddressBytes();
                    // flags from client DHCPDISCOVER message
                    // giaddr from client DHCPDISCOVER message
                    // chaddr from client DHCPDISCOVER message
                    sname.FillZero();
                    file.FillZero();
                    options.ApplyOptionSettings(msgType, server);
                    break;
                case DhcpMessgeType.DHCP_ACK:
                    op = (byte)BootMessageType.BootReply;
                    // htype
                    // hlen 
                    hops = 0;
                    // xid from client DHCPREQUEST message
                    secs.FillZero();
                    ciaddr.FillZero();
                    if (options.GetDhcpMessageType() == DhcpMessgeType.DHCP_INFORM)
                        yiaddr.FillZero();
                    else
                        yiaddr = IPAddress.Parse(clientIp.ToString()).GetAddressBytes();
                    siaddr = IPAddress.Parse("0.0.0.0").GetAddressBytes();
                    // flags from client DHCPREQUEST message
                    // giaddr from client DHCPREQUEST message
                    // chaddr from client DHCPREQUEST message
                    sname.FillZero();
                    file.FillZero();
                    options.ApplyOptionSettings(msgType, server);
                    break;
                case DhcpMessgeType.DHCP_NAK:
                    op = (byte)BootMessageType.BootReply;
                    // htype
                    // hlen 
                    hops = 0;
                    // xid from client DHCPREQUEST message
                    secs.FillZero();
                    // ciaddr from client DHCPREQUEST message
                    yiaddr.FillZero();
                    siaddr.FillZero();
                    // flags from client DHCPREQUEST message
                    // giaddr from client DHCPREQUEST message
                    // chaddr from client DHCPREQUEST message
                    sname.FillZero();
                    file.FillZero();
                    options.ApplyOptionSettings(msgType, server);
                    break;
            }
        }
        
        public byte[] ToArray()
        {
            var mArray = new byte[0];
            Utils.AddToArray(op, ref mArray);
            Utils.AddToArray(htype, ref mArray);
            Utils.AddToArray(hlen, ref mArray);
            Utils.AddToArray(hops, ref mArray);
            Utils.AddToArray(xid, ref mArray);
            Utils.AddToArray(secs, ref mArray);
            Utils.AddToArray(flags, ref mArray);
            Utils.AddToArray(ciaddr, ref mArray);
            Utils.AddToArray(yiaddr, ref mArray);
            Utils.AddToArray(siaddr, ref mArray);
            Utils.AddToArray(giaddr, ref mArray);
            Utils.AddToArray(chaddr, ref mArray);
            Utils.AddToArray(sname, ref mArray);
            Utils.AddToArray(file, ref mArray);
            Utils.AddToArray(cookie, ref mArray);
            Utils.AddToArray(options.options, ref mArray);
            return mArray;
        }

    #region Data
        public byte op;           // Op code:   1 = bootRequest, 2 = BootReply
        public byte htype;        // Hardware Address Type: 1 = 10MB ethernet
        public byte hlen;         // hardware address length: length of MACID
        public byte hops;         // Hw options
        public byte[] xid;        // transaction id (5)
        public byte[] secs;       // elapsed time from trying to boot (3)
        public byte[] flags;      // flags (3)
        public byte[] ciaddr;     // client IP (5)
        public byte[] yiaddr;     // your client IP (5)
        public byte[] siaddr;     // Server IP  (5)
        public byte[] giaddr;     // relay agent IP (5)
        public byte[] chaddr;     // Client HW address (16)
        public byte[] sname;      // Optional server host name (64)
        public byte[] file;       // Boot file name (128)
        public byte[] cookie;     // Magic cookie (4)
        public Options options;   // options (rest)
        #endregion
    }
}
