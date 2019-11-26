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

        public bool HasDevice
            => m_device != null;

        public MyNatDevice(int timeout)
        {
            m_timeout = timeout;
            m_discoverer = new NatDiscoverer();
            m_cancellationToken = new CancellationTokenSource(m_timeout);
        }

        public async Task FindDevice()
        {
            if (HasDevice) { return; }

            m_device = await m_discoverer.DiscoverDeviceAsync(PortMapper.Upnp, m_cancellationToken);
        }

        public async Task<List<MyNATMapping>> GetAllMappings()
        {
            var mappings = await m_device.GetAllMappingsAsync();
            if (mappings == null) { return new List<MyNATMapping>(); }

            List<MyNATMapping> returnVal = new List<MyNATMapping>(8);
            foreach (Mapping map in mappings)
            { returnVal.Add(new MyNATMapping(map)); }

            return returnVal;
        }

        public void CancelPendingRequests()
        {
            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
