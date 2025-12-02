using ASCOM.Common;

namespace ObsMan
{
    public class Device
    {
        /// <summary>
        /// Initializes a new instance of the Device class.
        /// </summary>
        public Device() { }

        public DeviceTechnology DeviceTechnology { get; set; } = DeviceTechnology.NotSelected;

        public string Name { get; set; } = string.Empty;
        public string AlpacaIpAddress { get; set; } = string.Empty;
        public int AlpacaPort { get; set; } = 0;
        public string DeviceId { get; set; } = string.Empty;
        public string ProgId { get; set; } = string.Empty;


        public DeviceTypes DeviceType { get; set; } = DeviceTypes.SafetyMonitor;


    }
}
