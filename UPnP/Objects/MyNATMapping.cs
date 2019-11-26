using Open.Nat;
using System;
using System.Collections.Generic;
using System.Text;

namespace UPnP.Objects
{
    public struct MyNATMapping
    {
#pragma warning disable IDE0032
        private Protocol m_protocol;
        private ushort m_privatePort;
        private ushort m_publicPort;
        private string m_description;
        private DateTime m_expiration;
#pragma warning restore

        public string Protocol
            => m_protocol.ToString().ToUpper();
        public ushort PrivatePort
            => m_privatePort;
        public ushort PublicPort
            => m_publicPort;
        public string Description
            => m_description;
        public string Expiration
            => m_expiration.ToString("dd/MM/yy   HH:mm:ss");

        public MyNATMapping(Mapping natMapping)
        {
            m_protocol = natMapping.Protocol;
            m_privatePort = (ushort)natMapping.PrivatePort;
            m_publicPort = (ushort)natMapping.PublicPort;
            m_description = natMapping.Description;
            m_expiration = natMapping.Expiration;
        }
    }
}
