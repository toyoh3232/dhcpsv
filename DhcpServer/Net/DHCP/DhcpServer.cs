using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Tohasoft.Net.DHCP.Internal;
using Tohasoft.Net.Extension;
using Tohasoft.Utils;

namespace Tohasoft.Net.DHCP
{
    public class DhcpServer : ILoggerEventSource
    {
        public delegate void DiscoverEventHandler(ClientInfomation data);
        public delegate void ReleasedEventHandler();
        public delegate void RequestEventHandler(ClientInfomation data);
        public delegate void AssignedEventHandler(string ipAdd);

        public event DiscoverEventHandler Discovered;
        public event RequestEventHandler Requested;

        public event EventHandler<WarningMessageEventArgs> WarningRaised;
        public event EventHandler<ErrorMessageEventArgs> ErrorRaised;
        public event EventHandler<MessageEventArgs> MessageRaised;

        // only broadcast, unicast w/out ARP is not supported
        // change to raw socket in future version
        private UdpListener udpListener; 

        public DhcpServerSettings _settings;

        private class OwnedIpAddress
        {
            public IPAddress Ip;
            public bool IsAllocated;
            public string AuthorizedClientMac;
        }

        // RFC 2131 p30-33
        private enum DhcpRequestType
        {
            Unknown,
            Selecting,
            InitReboot,
            ReNewing,
            ReBinding
        }

        private DhcpRequestType GetDhcpRequestType(ClientInfomation clientInfo, IPEndPoint endPoint)
        {
            DhcpRequestType res;
            if (clientInfo.ServerAddress != null && clientInfo.RequestAddress != null && clientInfo.ClientAddress.Equals(IPAddress.Any))
            {
                res = DhcpRequestType.Selecting;
            }
            else switch (clientInfo.ServerAddress)
            {
                case null when clientInfo.RequestAddress != null && clientInfo.ClientAddress.Equals(IPAddress.Any):
                    res = DhcpRequestType.InitReboot;
                    break;
                case null when clientInfo.RequestAddress == null && endPoint.Address.Equals(_settings.ServerIp):
                    res = DhcpRequestType.ReNewing;
                    break;
                case null when clientInfo.RequestAddress == null && endPoint.Address.Equals(IPAddress.Broadcast):
                    res = DhcpRequestType.ReBinding;
                    break;
                default:
                    res = DhcpRequestType.Unknown;
                    break;
            }
            return res;
        }

        private readonly List<OwnedIpAddress> ownedIpAddressPool;

        public DhcpServer(DhcpServerSettings settings)
        {
            // server ip is not set
            if (settings.ServerIp == null)
                throw new ArgumentNullException();

            // server ip is not binding to local nic
            if (!Utils.GetLocalIp().Contains(settings.ServerIp))
                throw new ArgumentOutOfRangeException();

            // ip range is set only one side
            if ((settings.StartIp == null) != (settings.EndIp == null))
                throw new ArgumentNullException();

            // given subnetmask is different to local machine settings
            if (!settings.SubnetMask?.Equals(settings.ServerIp.GetSubnetMask()) ?? false)
                throw new ArgumentOutOfRangeException();

            // reset the net mask
            settings.SubnetMask = settings.ServerIp.GetSubnetMask();

            // given ip range is not in a subnet
            if (!settings.StartIp?.IsInSameSubnet(settings.EndIp, settings.SubnetMask) ?? false )
                throw new ArgumentOutOfRangeException();

            // given ip range is not in an ascending range
            if (settings.StartIp?.ToLong() > settings.EndIp?.ToLong())
                throw new ArgumentOutOfRangeException();

            _settings = settings;

            // if ip is set in range
            if (_settings.StartIp != null)
            {
                var start = _settings.StartIp;
                var mask = _settings.SubnetMask;
                var server = _settings.ServerIp;

                ownedIpAddressPool = new List<OwnedIpAddress>();
                do
                {
                    if (!start.Equals(server))
                        ownedIpAddressPool.Add(new OwnedIpAddress { Ip = start, IsAllocated = false });
                }
                while ((start = start.Increment(mask)) != null);
            }
            // if no ip in range is set, get the default
            else
            {
                ownedIpAddressPool = _settings.ServerIp.GetAllSubnet(_settings.SubnetMask)
                .Select(ip => new OwnedIpAddress { Ip = ip, IsAllocated = false }).ToList();
            }
        }

        // function to start the DHCP server
        // port 67 to receive, 68 to send
        public void Start()
        {
            try
            {   // start the DHCP server
                // assign the event handlers
                udpListener = new UdpListener(67, 68, _settings.ServerIp.ToString());
                udpListener.Received += UdpListener_Received;

            }
            catch (Exception e)
            {
                ErrorRaised?.Invoke(this, new ErrorMessageEventArgs { Message = e.Message });
                throw e;
            }
        }

        public void Terminate()
        {
            udpListener.Received -= UdpListener_Received;
            udpListener.StopListener();
        }

        public void UdpListener_Received(byte[] data, IPEndPoint endPoint)
        {
            try
            {
                var dhcpData = new DhcpData(data)
                {
                    RelatedServer = this
                };
                var msgType = dhcpData.GetCurrentMessageType();
                var client = dhcpData.GetClientInfo();
                switch (msgType)
                {
                    case DhcpMessgeType.DHCP_DISCOVER:
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPDISCOVER received." });
                        Discovered?.Invoke(client);
                        var newIp = ownedIpAddressPool.Find(x =>(x.AuthorizedClientMac == client.MacAddress) || (x.IsAllocated == false && x.AuthorizedClientMac == null));
                        if (newIp.Ip == null)
                        {
                            MessageRaised?.Invoke(this, new MessageEventArgs { Message = "No ip is available to allocate." });
                            return;
                        }
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPOFFER sent." });
                        // MUST be unicast over raw socket (unimplemented)
                        // broadcast used
                        SendDhcpMessage(DhcpMessgeType.DHCP_OFFER, dhcpData, newIp);
                        break;

                    case DhcpMessgeType.DHCP_REQUEST:
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPREQUEST received." });
                        Requested?.Invoke(client);
                        switch (GetDhcpRequestType(client, endPoint))
                        {
                            // respond to client which has responded to DHCPOFFER message from this server 
                            case DhcpRequestType.Selecting:
                                MessageRaised?.Invoke(this, new MessageEventArgs { Message = "Response to DHCPREQUEST generated during SELECTING state." });
                                if (_settings.ServerIp.Equals(client.ServerAddress))
                                {
                                    var allocatedIp = ownedIpAddressPool.Find(x => x.Ip.Equals(client.RequestAddress));
                                    if (allocatedIp.Ip != null && !allocatedIp.IsAllocated)
                                    {
                                        allocatedIp.IsAllocated = true;
                                        allocatedIp.AuthorizedClientMac = client.MacAddress;
                                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPACK sent." });
                                        // broadcast
                                        SendDhcpMessage(DhcpMessgeType.DHCP_ACK, dhcpData, allocatedIp);
                                    }
                                }
                                break;
                            case DhcpRequestType.InitReboot:
                                MessageRaised?.Invoke(this, new MessageEventArgs { Message = "Response to DHCPREQUEST generated during INIT-REBOOT state." });
                                if (!client.RelayAgentAddress.Equals(IPAddress.Any))
                                    MessageRaised?.Invoke(this, new MessageEventArgs { Message = "Relay agent is not supported in this version." });
                                var rebootIp = ownedIpAddressPool.Find(x => x.Ip.Equals(client.RequestAddress));
                                if (rebootIp.Ip != null && rebootIp.AuthorizedClientMac == client.MacAddress)
                                {
                                    // broadcast
                                    SendDhcpMessage(DhcpMessgeType.DHCP_ACK, dhcpData, rebootIp);
                                    MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPACK sent." });
                                }
                                break;
                            case DhcpRequestType.ReNewing:
                                MessageRaised?.Invoke(this, new MessageEventArgs { Message = "Response to DHCPREQUEST generated during RENEWING state." });
                                var reNewIp = ownedIpAddressPool.Find(x => x.Ip.Equals(client.ClientAddress));
                                if (reNewIp.Ip != null && reNewIp.AuthorizedClientMac == client.MacAddress)
                                {
                                    // unicast
                                    SendDhcpMessage(client.ClientAddress.ToString(), DhcpMessgeType.DHCP_ACK, dhcpData, reNewIp);
                                    MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPACK sent." });
                                }
                                break;
                            case DhcpRequestType.ReBinding:
                                MessageRaised?.Invoke(this, new MessageEventArgs { Message = "Response to DHCPREQUEST generated during REBINDING state." });
                                var reBindIp = ownedIpAddressPool.Find(x => x.IsAllocated == false);
                                if (reBindIp.Ip != null)
                                {
                                    reBindIp.IsAllocated = true;
                                    reBindIp.AuthorizedClientMac = client.MacAddress;
                                    // broadcast
                                    SendDhcpMessage(DhcpMessgeType.DHCP_ACK, dhcpData, reBindIp);
                                    MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPACK sent." });
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case DhcpMessgeType.DHCP_DECLINE:
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPDECLINE received." });
                        var declinedIp = ownedIpAddressPool.Find(x => x.Ip.Equals(client.ClientAddress));
                        if (declinedIp.Ip != null) ownedIpAddressPool.Remove(declinedIp);
                        break;
                    case DhcpMessgeType.DHCP_RELEASE:
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPRELESE received." });
                        var releasedIp = ownedIpAddressPool.Find(x => x.Ip.Equals(client.ClientAddress));
                        if (releasedIp.Ip != null) releasedIp.IsAllocated = false;
                        break;
                    case DhcpMessgeType.DHCP_INFORM:
                        MessageRaised?.Invoke(this, new MessageEventArgs { Message = "DHCPINFORM received." });
                        // unicast
                        SendDhcpMessage(client.ClientAddress.ToString(), DhcpMessgeType.DHCP_ACK, dhcpData, null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                ErrorRaised?.Invoke(this, new ErrorMessageEventArgs { Message = e.Message });
                Terminate();
                throw e;
            }
        }

        // override method for broadcast 
        private void SendDhcpMessage(DhcpMessgeType msgType, DhcpData data, OwnedIpAddress newClient)
        {
            SendDhcpMessage(IPAddress.Broadcast.ToString(), msgType, data, newClient);
        }

        private void SendDhcpMessage(string dest, DhcpMessgeType msgType, DhcpData data, OwnedIpAddress newClient)
        {
            try
            {
                var dataToSend = data.BuildSendData(msgType, newClient.Ip);
                udpListener.SendData(dest, dataToSend);
            }
            catch (Exception e)
            {
                ErrorRaised?.Invoke(this, new ErrorMessageEventArgs { Message = e.Message });
            }
        }
    }
}
