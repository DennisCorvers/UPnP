using Open.Nat;
using System;
using System.Collections.Generic;
using System.Text;

namespace UPnP.Objects
{
    public struct MyNATMapping
    {
        public MyProtocol Protocol
        { get; }
        public ushort PrivatePort
        { get; }
        public ushort PublicPort
        { get; }
        public string Description
        { get; }
        public DateTime Expiration
        { get; }

        public MyNATMapping(Mapping natMapping)
        {
            Protocol = (MyProtocol)(int)natMapping.Protocol;
            PrivatePort = (ushort)natMapping.PrivatePort;
            PublicPort = (ushort)natMapping.PublicPort;
            Description = natMapping.Description;
            Expiration = natMapping.Expiration;
        }
    }
}
