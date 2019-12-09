using Open.Nat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UPnPWin.Exceptions;
using UPnPWin.Utils;

namespace UPnPWin.Objects
{
    internal sealed class MyNatDevice
    {
        public const bool TestDuplicates = true;
        public const int Timeout = 5000;

        private NatDevice m_device;
        private NatDiscoverer m_discoverer;
        private readonly CancellationTokenSource m_cancellationToken;

        private ThreadSafeBool m_deviceAvailable;

        public IPAddress PublicIP
        { get; private set; }

        public IPEndPoint DeviceEndpoint
        {
            get
            {
                if (m_device == null)
                    return null;
                return AddressReflection.GetEndpoint(m_device);
            }
        }
        public IPAddress LocalIP
        {
            get
            {
                if (m_device == null)
                    return null;
                return AddressReflection.GetLocalIPAddress(m_device);
            }
        }

        public static bool HasDevice
            => Instance.m_deviceAvailable.Value;
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
            m_deviceAvailable.Value = false;
            m_cancellationToken = new CancellationTokenSource(Timeout);
        }

        public async Task FindDevice()
        {
            if (m_deviceAvailable.Value) { return; }

            m_discoverer = new NatDiscoverer();
            Task<NatDevice> discoverDeviceTask = m_discoverer.DiscoverDeviceAsync(PortMapper.Upnp, m_cancellationToken);
            m_device = await discoverDeviceTask;

            if (m_device == null)
                PublicIP = IPAddress.Any;
            else
                PublicIP = await m_device.GetExternalIPAsync();

            if (discoverDeviceTask.IsCompleted)
                m_deviceAvailable.Value = true;
        }
        public async Task<List<MyNatMapping>> GetAllMappings()
        {
            if (!m_deviceAvailable.Value)
                throw new NatDeviceNotFoundException();

            IEnumerable<Mapping> mappings;
            try
            { mappings = await m_device.GetAllMappingsAsync(); }
            catch (NatDeviceNotFoundException e)
            { m_deviceAvailable.Value = false; throw e; }

            if (mappings == null)
                return new List<MyNatMapping>();

            List<MyNatMapping> newMappings = new List<MyNatMapping>();
            foreach (var map in mappings)
                newMappings.Add(new MyNatMapping(map));

            return newMappings;
        }

        public async Task AddMappings(List<Mapping> mappings)
        {
            if (!HasDevice) { throw new NatDeviceNotFoundException(); }

            var tasks = new List<Task>(mappings.Count);
            foreach (var map in mappings)
                tasks.Add(AddMapping(map));

            var task = Task.WhenAll(tasks);

            try
            { await task; }
            catch (Exception)
            {
                if (task.Exception != null)
                    throw task.Exception.InnerException;
            }
        }
        private async Task AddMapping(Mapping mapping)
        {
            if (!m_deviceAvailable.Value)
                throw new NatDeviceNotFoundException();

            Mapping dupeMapping;
            try
            {
                if (TestDuplicates && (dupeMapping = await m_device.GetSpecificMappingAsync
                        (mapping.Protocol, mapping.PrivatePort)) != null)
                    throw new DuplicateMappingException(dupeMapping);

                await m_device.CreatePortMapAsync(mapping);
            }
            catch (NatDeviceNotFoundException e)
            { m_deviceAvailable.Value = false; throw e; }
        }
        public async Task RemoveMappings(List<Mapping> mappings)
        {
            //TODO Test when no network/UPnP available...
            if (!m_deviceAvailable.Value) { throw new NatDeviceNotFoundException(); }

            var tasks = new List<Task>(mappings.Count);
            foreach (var map in mappings)
                tasks.Add(RemoveMapping(map));

            var task = Task.WhenAll(tasks);

            try
            { await task; }
            catch (Exception)
            {
                if (task.Exception != null)
                    throw task.Exception.InnerException;
            }
        }
        private async Task RemoveMapping(Mapping mapping)
        {
            if (!m_deviceAvailable.Value)
                throw new NatDeviceNotFoundException();

            try
            { await m_device.DeletePortMapAsync(mapping); }
            catch (NatDeviceNotFoundException e)
            { m_deviceAvailable.Value = false; throw e; }
        }

        public void CancelPendingRequests()
        {
            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
