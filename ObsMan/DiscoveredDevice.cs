using ASCOM;
using ASCOM.Com;
using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ObsMan
{
    public class DiscoveredDevice
    {
        private IObservingConditionsV2? observingConditionsDevice;
        private ISwitchV3? switchDevice;
        private ISafetyMonitorV3? safetyMonitorDevice;
        private string guid = Guid.NewGuid().ToString();

        public string Id
        {
            get
            {
                return guid;
            }
        }

        public ObsManDeviceType ObsManDeviceType { get; set; } = ObsManDeviceType.NotSet;
        public string Name { get; set; } = "UnknownName";
        public string HostName { get; set; } = "UnknownHostName";
        public int IpPort { get; set; } = 0;
        public string ProgID { get; set; } = "UnknownProgID";

        public string DisplayName
        {
            get
            {
                switch (Protocol)
                {
                    case Protocol.Alpaca:
                        return $"{Name} ({HostName}:{IpPort})";

                    case Protocol.COM:
                        return $"{Name} ({ProgID})";

                    default:
                        return "";
                }
            }
        }

        public Protocol Protocol { get; set; } = Protocol.Unknown;

        public void CreateObservingConditionsDevice()
        {
            if (observingConditionsDevice is null)
            {
                switch (ObsManDeviceType)
                {
                    case ObsManDeviceType.ObservingConditions:
                        if (observingConditionsDevice is null)
                        {
                            switch (Protocol)
                            {
                                case Protocol.Alpaca:
                                    ASCOM.Alpaca.Clients.AlpacaConfiguration config = new()
                                    {
                                        IpAddressString = HostName,
                                        PortNumber = IpPort,

                                        UserAgentProductName = Globals.USER_AGENT_PRODUCT_NAME
                                    };
                                    observingConditionsDevice = new ASCOM.Alpaca.Clients.AlpacaObservingConditions(config);
                                    break;
#if WINDOWS
                                    case Protocol.COM:
                                        observingConditionsDevice = new ASCOM.Com.DriverAccess.ObservingConditions(ProgID);
                                        break;
#endif
                                default:
                                    throw new InvalidValueException("Protocol not supported.");
                            }
                        }
                        break;

                    case ObsManDeviceType.SafetyMonitor:
                    case ObsManDeviceType.Switch:
                        throw new InvalidValueException($"This is an ObservingConditions device not a {ObsManDeviceType} device.");

                    // Default value so do nothing
                    case ObsManDeviceType.NotSet:
                        break;

                    default:
                        throw new InvalidValueException($"Device type {ObsManDeviceType} not supported.");
                }
            }

        }

        public IObservingConditionsV2? ObservingConditions { get; set; } = null;

        public ISwitchV3? Switch { get; set;} = null;

        public void CreateSafetyMonitorDevice()
        {
            if (safetyMonitorDevice is null)
            {
                switch (ObsManDeviceType)
                {
                    case ObsManDeviceType.SafetyMonitor:
                        if (safetyMonitorDevice is null)
                        {
                            switch (Protocol)
                            {
                                case Protocol.Alpaca:
                                    ASCOM.Alpaca.Clients.AlpacaConfiguration config = new()
                                    {
                                        IpAddressString = HostName,
                                        PortNumber = IpPort,

                                        UserAgentProductName = Globals.USER_AGENT_PRODUCT_NAME
                                    };
                                    safetyMonitorDevice = new ASCOM.Alpaca.Clients.AlpacaSafetyMonitor(config);
                                    break;
#if WINDOWS
                                    case Protocol.COM:
                                        safetyMonitorDevice = new ASCOM.Com.DriverAccess.SafetyMonitor(ProgID);
                                        break;
#endif
                                default:
                                    throw new InvalidValueException("Protocol not supported.");
                            }
                        }
                        break;

                    case ObsManDeviceType.ObservingConditions:
                    case ObsManDeviceType.Switch:
                        throw new InvalidValueException($"This is a SafetyMonitor device not a {ObsManDeviceType} device.");

                    // Default value so do nothing
                    case ObsManDeviceType.NotSet:
                        break;

                    default:
                        throw new InvalidValueException($"Device type {ObsManDeviceType} not supported.");
                }
            }
        }
        public ISafetyMonitorV3? Safetymonitor { get; set; } = null;
    }
}
