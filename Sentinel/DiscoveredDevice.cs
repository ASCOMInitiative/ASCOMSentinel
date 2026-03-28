namespace Sentinel
{
    public class DiscoveredDevice
    {
        /// <summary>
        /// Communication protocol e.g. Alpaca, COM etc.
        /// </summary>
        public Protocol Protocol { get; set; } = Protocol.NotConfigured;

        /// <summary>
        /// Device type for this device.
        /// </summary>
        public SentinelDeviceType SentinelDeviceType { get; set; } = SentinelDeviceType.NotSet;

        /// <summary>
        /// Device name.
        /// </summary>
        public string Name { get; set; } = "Not configured";

        /// <summary>
        /// Alpaca host name or IP address.
        /// </summary>
        public string IpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// IP port for the device.
        /// </summary>
        public int PortNumber { get; set; } = 32323;

        /// <summary>
        /// Alpaca device number
        /// </summary>
        public int RemoteDeviceNumber { get; set; } = 0;

        /// <summary>
        /// COM ProgID for the device.
        /// </summary>
        public string ComProgID { get; set; } = "Not configured";

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
                        return $"{Name} ({IpAddress}:{PortNumber} Device {RemoteDeviceNumber})";

                    case Protocol.COM:
                        return $"{Name} ({ComProgID})";

                    default:
                        switch (this.SentinelDeviceType)
                        {
                            case SentinelDeviceType.ManualObservingConditions:
                            case SentinelDeviceType.ManualSafetyMonitor:
                                return $"New Manual Configuration";

                            default:
                                return Globals.NOT_SET;
                        }
                }
            }
        }
    }
}
