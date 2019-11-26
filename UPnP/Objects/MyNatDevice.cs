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
        private Task m_currentTask;
        private object m_lock;

        public bool HasDevice
            => m_device != null;
        public bool IsRunning
        {
            get
            {
                lock (m_lock)
                { return m_currentTask.Status != TaskStatus.Running; }
            }
        }

        public MyNatDevice(int timeout)
        {
            m_timeout = timeout;
            m_lock = new object();
            m_discoverer = new NatDiscoverer();
            m_cancellationToken = new CancellationTokenSource(m_timeout);
        }

        private Task<T> RegisterTask<T>(Task<T> newTask)
        {
            lock (m_lock)
            {
                if (m_currentTask != null)
                {
                    if (m_currentTask.Status == TaskStatus.Running
                        || m_currentTask.Status == TaskStatus.WaitingForActivation)
                    { throw new AlreadyWorkingException(); }
                }

                m_currentTask = newTask;
                return newTask;
            }
        }

        public async Task FindDevice()
        {
            if (HasDevice) { return; }

            m_device = await RegisterTask(
                m_discoverer.DiscoverDeviceAsync(
                    PortMapper.Upnp, m_cancellationToken));
        }

        public async Task<List<MyNATMapping>> GetAllMappings()
        {
            var mappings = await RegisterTask(m_device.GetAllMappingsAsync());
            if (mappings == null) { return new List<MyNATMapping>(); }

            List<MyNATMapping> returnVal = new List<MyNATMapping>(8);
            foreach (Mapping map in mappings)
            { returnVal.Add(new MyNATMapping(map)); }

            return returnVal;
        }

        public void CancelPendingRequests()
        {
            if (!IsRunning) { return; }

            if (m_cancellationToken != null)
            { m_cancellationToken.Cancel(); }
        }
    }
}
