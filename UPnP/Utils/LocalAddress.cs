using Open.Nat;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UPnP.Objects;

namespace UPnP.Utils
{
    internal static class LocalAddress
    {
        private const string UpnpNatDeviceName = "Open.Nat.UpnpNatDevice";
        private const string UpnpNatDeviceInfoName = "Open.Nat.UpnpNatDeviceInfo";

        private static readonly Assembly m_openNatAssem;
        private static readonly Type m_upnpDeviceType;
        private static readonly Type m_deviceInfoType;

        private static Func<NatDevice, object> GetDeviceInfo;
        private static Func<object, IPAddress> GetPrivateIPAddress;
        private static Func<object, IPEndPoint> GetDeviceIPAddress;

        static LocalAddress()
        {
            m_openNatAssem = typeof(NatDevice).Assembly;

            m_upnpDeviceType = m_openNatAssem.GetType(UpnpNatDeviceName);
            m_deviceInfoType = m_openNatAssem.GetType(UpnpNatDeviceInfoName);

            FieldInfo field = GetPrivateField(m_upnpDeviceType, "DeviceInfo");
            GetDeviceInfo = field.CreateGetter<NatDevice, object>();

            field = GetPrivateField(m_deviceInfoType, "<LocalAddress>k__BackingField");
            GetPrivateIPAddress = field.CreateGetter<object, IPAddress>();

            field = GetPrivateField(m_deviceInfoType, "<LocalAddress>k__BackingField");
            GetDeviceIPAddress = field.CreateGetter<object, IPEndPoint>();
        }

        private static FieldInfo GetPrivateField(Type type, string name)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static IPAddress GetLocalIPAddress(this NatDevice myDevice)
        {
            //TODO backup for  when reflection is not available.
            return GetPrivateIPAddress(GetDeviceInfo(myDevice));
        }
        internal static IPEndPoint GetEndpoint(this NatDevice myDevice)
        {
            //TODO backup for  when reflection is not available.
            return GetDeviceIPAddress(GetDeviceInfo(myDevice));
        }
    }
}
