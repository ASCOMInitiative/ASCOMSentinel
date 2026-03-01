using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;

namespace ObsMan
{
    public class Device : ISafetyMonitorV3, IObservingConditionsV2, ISwitchV3, IDisposable
    {
        private bool disposedValue;

        #region Initialisers and dispose

        /// <summary>
        /// Initializes a new instance of the Device class with the given device number.
        /// </summary>
        /// <param name="deviceNumber">The unique number of this instance.</param>
        public Device(int deviceNumber)
        {
            DeviceNumber = deviceNumber;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finaliser
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Information and management methods

        public DeviceTechnology DeviceTechnology { get; set; } = DeviceTechnology.NotSelected;

        public int DeviceNumber { get; private set; }

        public string Name { get; set; } = string.Empty;

        public string AlpacaIpAddress { get; set; } = string.Empty;

        public int AlpacaPort { get; set; } = 0;

        public string DeviceId { get; set; } = string.Empty;

        public string ProgId { get; set; } = string.Empty;

        public ObsManDeviceType ObsManDeviceType { get; set; } = ObsManDeviceType.SafetyMonitor;

        #endregion

        #region Common methods

        public bool Connecting => throw new NotImplementedException();

        public List<StateValue> DeviceState => throw new NotImplementedException();

        public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Description => throw new NotImplementedException();

        public string DriverInfo => throw new NotImplementedException();

        public string DriverVersion => throw new NotImplementedException();

        public short InterfaceVersion => throw new NotImplementedException();

        public IList<string> SupportedActions => throw new NotImplementedException();

        public string Action(string actionName, string actionParameters)
        {
            throw new NotImplementedException();
        }

        public void CommandBlind(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }

        public bool CommandBool(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }

        public string CommandString(string command, bool raw = false)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Safety Monitor

        bool ISafetyMonitor.IsSafe => throw new NotImplementedException();

        #endregion

        #region Observing Conditions

        double IObservingConditions.AveragePeriod { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        double IObservingConditions.CloudCover => throw new NotImplementedException();

        double IObservingConditions.DewPoint => throw new NotImplementedException();

        double IObservingConditions.Humidity => throw new NotImplementedException();

        double IObservingConditions.Pressure => throw new NotImplementedException();

        double IObservingConditions.RainRate => throw new NotImplementedException();

        double IObservingConditions.SkyBrightness => throw new NotImplementedException();

        double IObservingConditions.SkyQuality => throw new NotImplementedException();

        double IObservingConditions.StarFWHM => throw new NotImplementedException();

        double IObservingConditions.SkyTemperature => throw new NotImplementedException();

        double IObservingConditions.Temperature => throw new NotImplementedException();

        double IObservingConditions.WindDirection => throw new NotImplementedException();

        double IObservingConditions.WindGust => throw new NotImplementedException();

        double IObservingConditions.WindSpeed => throw new NotImplementedException();

        void IObservingConditions.Refresh()
        {
            throw new NotImplementedException();
        }

        string IObservingConditions.SensorDescription(string PropertyName)
        {
            throw new NotImplementedException();
        }

        double IObservingConditions.TimeSinceLastUpdate(string PropertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Switch

        short ISwitchV2.MaxSwitch => throw new NotImplementedException();

        bool ISwitchV3.CanAsync(short id)
        {
            throw new NotImplementedException();
        }

        void ISwitchV3.CancelAsync(short id)
        {
            throw new NotImplementedException();
        }

        bool ISwitchV2.CanWrite(short id)
        {
            throw new NotImplementedException();
        }

        bool ISwitchV2.GetSwitch(short id)
        {
            throw new NotImplementedException();
        }

        string ISwitchV2.GetSwitchDescription(short id)
        {
            throw new NotImplementedException();
        }

        string ISwitchV2.GetSwitchName(short id)
        {
            throw new NotImplementedException();
        }

        double ISwitchV2.GetSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        double ISwitchV2.MaxSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        double ISwitchV2.MinSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        void ISwitchV3.SetAsync(short id, bool state)
        {
            throw new NotImplementedException();
        }

        void ISwitchV3.SetAsyncValue(short id, double value)
        {
            throw new NotImplementedException();
        }

        void ISwitchV2.SetSwitch(short id, bool state)
        {
            throw new NotImplementedException();
        }

        void ISwitchV2.SetSwitchName(short id, string name)
        {
            throw new NotImplementedException();
        }

        void ISwitchV2.SetSwitchValue(short id, double value)
        {
            throw new NotImplementedException();
        }

        bool ISwitchV3.StateChangeComplete(short id)
        {
            throw new NotImplementedException();
        }

        double ISwitchV2.SwitchStep(short id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
