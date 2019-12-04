using Open.Nat;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UPnP.Utils;

namespace UPnP.Objects
{
    internal sealed class MyNatDevice
    {
        public const int Timeout = 5000;

        private NatDevice m_device;
        private readonly NatDiscoverer m_discoverer;
        private readonly CancellationTokenSource m_cancellationToken;
        private readonly List<MyNatMapping> m_mappings;

        public IPAddress PublicIP
        { get; private set; }

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
            if (!HasDevice) { throw new NatDeviceNotFoundException(); }

            m_mappings.Clear();

            IEnumerable<Mapping> mappings;
            try
            { mappings = await m_device.GetAllMappingsAsync(); }
            catch (NatDeviceNotFoundException e)
            { m_device = null; throw e; }

            if (mappings == null) { return m_mappings; }

            foreach (var map in mappings)
            { m_mappings.Add(new MyNatMapping(map)); }

            return m_mappings;
        }

        public async Task AddMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NatDeviceNotFoundException(); }

            try
            { await m_device.CreatePortMapAsync(mapping); }
            catch (NatDeviceNotFoundException e)
            { m_device = null; throw e; }
        }
        public async Task RemoveMapping(Mapping mapping)
        {
            if (!HasDevice) { throw new NatDeviceNotFoundException(); }

            try
            { await m_device.DeletePortMapAsync(mapping); }
            catch (NatDeviceNotFoundException e)
            { m_device = null; throw e; }
        }

        public void CancelPendingRequests()
        {
            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
