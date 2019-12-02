using Open.Nat;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UPnP.Exceptions;
using UPnP.Utils;

namespace UPnP.Objects
{
    internal static class MyNatDevice
    {
        public const int Timeout = 5000;

        private static NatDiscoverer m_discoverer;
        private static NatDevice m_device; //DeviceInfo > LocalDevice
        private static CancellationTokenSource m_cancellationToken;

        private static List<MyNatMapping> m_mappings;

        public static bool HasDevice
            => m_device != null;
        public static IPAddress PublicIP
        { private get; set; }

        public static IPEndPoint DeviceEndpoint
        {
            get
            {
                if (m_device == null)
                    return null;
                return LocalAddress.GetEndpoint(m_device);
            }
        }
        public static IPAddress LocalIP
        {
            get
            {
                if (m_device == null)
                    return null;
                return LocalAddress.GetLocalIPAddress(m_device);
            }
        }


        static MyNatDevice()
        {
            m_mappings = new List<MyNatMapping>(8);
            m_discoverer = new NatDiscoverer();
            m_cancellationToken = new CancellationTokenSource(Timeout);
        }

        public static async Task FindDevice()
        {
            if (HasDevice) { return; }

            m_device = await m_discoverer.DiscoverDeviceAsync(PortMapper.Upnp, m_cancellationToken);

            if (m_device == null)
                PublicIP = IPAddress.Any;
            else
                PublicIP = await m_device.GetExternalIPAsync();
        }

        public static async Task<List<MyNatMapping>> GetAllMappings()
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            m_mappings.Clear();

            var mappings = await m_device.GetAllMappingsAsync();
            if (mappings == null) { return m_mappings; }

            foreach (var map in mappings)
            { m_mappings.Add(new MyNatMapping(map)); }

            return m_mappings;
        }

        public static async Task AddMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            await m_device.CreatePortMapAsync(mapping);
        }
        public static async Task RemoveMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            await m_device.DeletePortMapAsync(mapping);
        }

        public static void CancelPendingRequests()
        {
            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
