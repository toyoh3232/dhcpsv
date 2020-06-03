namespace Tohasoft.Net.DHCP.Internal
{
    // BOOTP message type
    public enum BootMessageType
    {
        BootRequest = 1,
        BootReply = 2
    }

    public enum BroadCastType
    {
        BroadCast = 1,
        UniCast = 0
    }

    // DHCP message type in options
    public enum DhcpMessgeType
    {
        DHCP_DISCOVER = 1,
        DHCP_OFFER = 2,
        DHCP_REQUEST = 3,
        DHCP_DECLINE = 4,
        DHCP_ACK = 5,
        DHCP_NAK = 6,
        DHCP_RELEASE = 7,
        DHCP_INFORM = 8
    }

    // refer to the rfc2132.txt for vendor specific info
    internal enum DhcpOptionType
    {
        Pad = 0,
        SubnetMask = 1,
        TimeOffset = 2,
        Router = 3,
        TimeServer = 4,
        NameServer = 5,
        DomainNameServer = 6,
        LogServer = 7,
        CookieServer = 8,
        LPRServer = 9,
        ImpressServer = 10,
        ResourceLocServer = 11,
        HostName = 12,
        BootFileSize = 13,
        MeritDump = 14,
        DomainName = 15,
        SwapServer = 16,
        RootPath = 17,
        ExtensionsPath = 18,
        IpForwarding = 19,
        NonLocalSourceRouting = 20,
        PolicyFilter = 21,
        MaximumDatagramReAssemblySize = 22,
        DefaultIPTimeToLive = 23,
        PathMTUAgingTimeout = 24,
        PathMTUPlateauTable = 25,
        InterfaceMTU = 26,
        AllSubnetsAreLocal = 27,
        BroadcastAddress = 28,
        PerformMaskDiscovery = 29,
        MaskSupplier = 30,
        PerformRouterDiscovery = 31,
        RouterSolicitationAddress = 32,
        StaticRoute = 33,
        TrailerEncapsulation = 34,
        ARPCacheTimeout = 35,
        EthernetEncapsulation = 36,
        TCPDefaultTTL = 37,
        TCPKeepaliveInterval = 38,
        TCPKeepaliveGarbage = 39,
        NetworkInformationServiceDomain = 40,
        NetworkInformationServers = 41,
        NetworkTimeProtocolServers = 42,
        VendorSpecificInformation = 43,
        NetBIOSoverTCPIPNameServer = 44,
        NetBIOSoverTCPIPDatagramDistributionServer = 45,
        NetBIOSoverTCPIPNodeType = 46,
        NetBIOSoverTCPIPScope = 47,
        XWindowSystemFontServer = 48,
        XWindowSystemDisplayManager = 49,
        RequestedIPAddress = 50,
        IPAddressLeaseTime = 51,
        OptionOverload = 52,
        DHCPMessageType = 53,
        ServerIdentifier = 54,
        ParameterRequestList = 55,
        Message = 56,
        MaximumDHCPMessageSize = 57,
        RenewalTimeValue_T1 = 58,
        RebindingTimeValue_T2 = 59,
        Vendorclassidentifier = 60,
        ClientIdentifier = 61,
        NetworkInformationServicePlusDomain = 64,
        NetworkInformationServicePlusServers = 65,
        TFTPServerName = 66,
        BootfileName = 67,
        MobileIPHomeAgent = 68,
        SMTPServer = 69,
        POP3Server = 70,
        NNTPServer = 71,
        DefaultWWWServer = 72,
        DefaultFingerServer = 73,
        DefaultIRCServer = 74,
        StreetTalkServer = 75,
        STDAServer = 76,
        End = 255
    }
}

