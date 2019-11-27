using Open.Nat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UPnP.Exceptions;

namespace UPnP.Objects
{
    public class MyNatDevice
    {
        private readonly int m_timeout;

        private NatDiscoverer m_discoverer;
        private NatDevice m_device;
        private CancellationTokenSource m_cancellationToken;

        private List<MyNatMapping> m_mappings;

        public bool HasDevice
            => m_device != null;

        public MyNatDevice(int timeout)
        {
            m_mappings = new List<MyNatMapping>(8);
            m_timeout = timeout;
            m_discoverer = new NatDiscoverer();
            m_cancellationToken = new CancellationTokenSource(m_timeout);
        }

        public async Task FindDevice()
        {
            if (HasDevice) { return; }

            m_device = await m_discoverer.DiscoverDeviceAsync(PortMapper.Upnp, m_cancellationToken);
        }

        public async Task<List<MyNatMapping>> GetAllMappings()
        {
            if(!HasDevice) { throw new NoNatDeviceException(); }

            m_mappings.Clear();

            var mappings = await m_device.GetAllMappingsAsync();
            if (mappings == null) { return m_mappings; }

            foreach (var map in mappings)
            { m_mappings.Add(new MyNatMapping(map)); }

            return m_mappings;
        }

        public async Task AddMapping(Mapping mapping)
        {
            if(!HasDevice) { throw new NoNatDeviceException(); }

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
