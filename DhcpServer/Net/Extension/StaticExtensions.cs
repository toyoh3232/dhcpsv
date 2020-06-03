using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Tohasoft.Net.Extension
{
    public static class StaticExtensions
    {

        public static IPAddress[] GetAllSubnet(this IPAddress hostIP, IPAddress netmask)
        {
            var IPs = new List<IPAddress>();
            var hostInt = hostIP.ToLong();
            var netmaskInt = netmask.ToLong();
            var wildCard = ~(uint)netmaskInt;
            var netBytesInt = hostInt & netmaskInt;
            for (var start = 1; start <= wildCard - 1; start++)
            {
                var ip = new IPAddress((netBytesInt + start).ToBytes());
                if (!ip.Equals(hostIP)) IPs.Add(ip);
            }
            return IPs.ToArray();
        }

        public static IPAddress Increment(this IPAddress hostIP, IPAddress netmask)
        {
            var hostInt = hostIP.ToLong();
            var netmaskInt = netmask.ToLong();
            var wildCard = ~(uint)netmaskInt;
            var netBytesInt = hostInt & netmaskInt;
            if (hostInt == netBytesInt + wildCard - 1)
                return null;
            return new IPAddress(hostInt + 1);
        }

        public static long ToLong(this IPAddress address)
        {
            uint ip = 0;
            var data = address.GetAddressBytes();
            for (var i = 0; i < data.Length; i++)
            {
                ip += ((uint)data[i]) << (8 * (data.Length - i - 1));
            }
            return ip;
        }

        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetSubnetMask(this IPAddress address)
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var addressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (addressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.ToString().Equals(addressInformation.Address.ToString()))
                        {
                            return addressInformation.IPv4Mask;
                        }
                    }
                }
            }
            return null;
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        public static void FillZero(this byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
        }

        public static string ToString(this byte[] data, int len)
        {
            var dString = string.Empty;
            if (data == null) return dString;
            for (var i = 0; i < len; i++)
            {
                dString += data[i].ToString("X2");
            }
            return dString;
        }

        public static byte[] ToBytes(this long data)
        {
            var bytes = new byte[4];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(data >> (8 * (bytes.Length - i - 1)));
            }
            return bytes;
        }
    }
}
