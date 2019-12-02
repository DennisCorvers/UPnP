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
    internal sealed class MyNatDevice
    {
        public const int Timeout = 5000;

        private NatDiscoverer m_discoverer;
        private NatDevice m_device;
        private CancellationTokenSource m_cancellationToken;
        private List<MyNatMapping> m_mappings;

        public IPAddress PublicIP
        { private get; set; }

        public IPEndPoint DeviceEndpoint
        {
            get
            {
                if (m_device == null)
                    return null;
                return LocalAddress.GetEndpoint(m_device);
            }
        }
        public IPAddress LocalIP
        {
            get
            {
                if (m_device == null)
                    return null;
                return LocalAddress.GetLocalIPAddress(m_device);
            }
        }

        public static bool HasDevice
            => Instance.m_device != null;
        private static MyNatDevice m_instance = null;
        private static readonly object m_lock = new object();

        public static MyNatDevice Instance
        {
            get
            {
                lock (m_lock)
                {
                    if (m_instance == null)
                        m_instance = new MyNatDevice();
                    return m_instance;
                }
            }
        }

        private MyNatDevice()
        {
            m_mappings = new List<MyNatMapping>(8);
            m_discoverer = new NatDiscoverer();
            m_cancellationToken = new CancellationTokenSource(Timeout);
        }

        public async Task FindDevice()
        {
            if (HasDevice) { return; }

            m_device = await m_discoverer.DiscoverDeviceAsync(PortMapper.Upnp, m_cancellationToken);

            if (m_device == null)
                PublicIP = IPAddress.Any;
            else
                PublicIP = await m_device.GetExternalIPAsync();
        }
        public async Task<List<MyNatMapping>> GetAllMappings()
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            m_mappings.Clear();

            var mappings = await m_device.GetAllMappingsAsync();
            if (mappings == null) { return m_mappings; }

            foreach (var map in mappings)
            { m_mappings.Add(new MyNatMapping(map)); }

            return m_mappings;
        }

        public async Task AddMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            await m_device.CreatePortMapAsync(mapping);
        }
        public async Task RemoveMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NoNatDeviceException(); }

            await m_device.DeletePortMapAsync(mapping);
        }

        public void CancelPendingRequests()
        {
            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
