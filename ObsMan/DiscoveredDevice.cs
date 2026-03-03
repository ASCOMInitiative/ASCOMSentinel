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

        /// <summary>
        /// Communication protocol e.g. Alpaca, COM etc.
        /// </summary>
        public Protocol Protocol { get; set; } = Protocol.NotConfigured;

        /// <summary>
        /// Device type for this device.
        /// </summary>
        public ObsManDeviceType ObsManDeviceType { get; set; } = ObsManDeviceType.NotSet;

        /// <summary>
        /// Device name.
        /// </summary>
        public string Name { get; set; } = "UnknownName";

        /// <summary>
        /// Alapca host name or IP address.
        /// </summary>
        public string IpAddress { get; set; } = "UnknownHostName";

        /// <summary>
        /// IP port for the device.
        /// </summary>
        public int PortNumber { get; set; } = 0;

        /// <summary>
        /// Alpaca device number
        /// </summary>
        public int RemoteDeviceNumber { get; set; } = 0;

        /// <summary>
        /// COM ProgID for the device.
        /// </summary>
        public string ComProgID { get; set; } = "UnknownProgID";

        /// <summary>
        /// Formatted name for use in UI
        /// </summary>
        public string DisplayName
        {
            get
            {
                switch (Protocol)
                {
                    case Protocol.Alpaca:
                        return $"{Name} ({IpAddress}:{PortNumber})";

                    case Protocol.COM:
                        return $"{Name} ({ComProgID})";

                    default:
                        return "Not set";
                }
            }
        }

        #region Device instance management

        public IObservingConditionsV2? ObservingConditions { get; set; } = null;

        public ISafetyMonitorV3? Safetymonitor { get; set; } = null;

        public ISwitchV3? Switch { get; set;} = null;

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
                                        IpAddressString = IpAddress,
                                        PortNumber = PortNumber,

                                        UserAgentProductName = Globals.USER_AGENT_PRODUCT_NAME
                                    };
                                    observingConditionsDevice = new ASCOM.Alpaca.Clients.AlpacaObservingConditions(config);
                                    break;
#if WINDOWS
                                    case Protocol.COM:
                                        observingConditionsDevice = new ASCOM.Com.DriverAccess.ObservingConditions(ComProgID);
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
                                        IpAddressString = IpAddress,
                                        PortNumber = PortNumber,

                                        UserAgentProductName = Globals.USER_AGENT_PRODUCT_NAME
                                    };
                                    safetyMonitorDevice = new ASCOM.Alpaca.Clients.AlpacaSafetyMonitor(config);
                                    break;
#if WINDOWS
                                    case Protocol.COM:
                                        safetyMonitorDevice = new ASCOM.Com.DriverAccess.SafetyMonitor(ComProgID);
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

        #endregion
    }
}
