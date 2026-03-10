namespace ObsMan
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
                        return Globals.NOT_SET;
                }
            }
        }
    }
}
