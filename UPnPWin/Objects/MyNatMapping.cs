using Open.Nat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UPnPWin.Objects
{
    internal struct MyNatMapping
    {
#pragma warning disable IDE0032
        private Mapping m_mapping;
#pragma warning restore

        public string Protocol
            => m_mapping.Protocol.ToString().ToUpper();
        public IPAddress IPAddress
            => m_mapping.PrivateIP;
        public int PrivatePort
            => m_mapping.PrivatePort;
        public int PublicPort
            => m_mapping.PublicPort;
        public string Description
            => m_mapping.Description;
        public string Expiration
            => m_mapping.Expiration.ToString("dd/MM/yy   HH:mm:ss");

        public Mapping Mapping
        {
            get => m_mapping;
            private set => m_mapping = value;
        }

        public MyNatMapping(Mapping natMapping)
        {
            if (natMapping == null)
            { throw new ArgumentNullException("natMapping"); }

            m_mapping = natMapping;
        }
    }
}
