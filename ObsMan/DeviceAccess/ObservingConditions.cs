using ASCOM.Common.DeviceInterfaces;

namespace ObsMan.DeviceAccess
{
    public class ObservingConditions : IObservingConditionsV2
    {
        public bool Connecting => throw new NotImplementedException();

        public List<StateValue> DeviceState => throw new NotImplementedException();

        public double AveragePeriod { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double CloudCover => throw new NotImplementedException();

        public double DewPoint => throw new NotImplementedException();

        public double Humidity => throw new NotImplementedException();

        public double Pressure => throw new NotImplementedException();

        public double RainRate => throw new NotImplementedException();

        public double SkyBrightness => throw new NotImplementedException();

        public double SkyQuality => throw new NotImplementedException();

        public double StarFWHM => throw new NotImplementedException();

        public double SkyTemperature => throw new NotImplementedException();

        public double Temperature => throw new NotImplementedException();

        public double WindDirection => throw new NotImplementedException();

        public double WindGust => throw new NotImplementedException();

        public double WindSpeed => throw new NotImplementedException();

        public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Description => throw new NotImplementedException();

        public string DriverInfo => throw new NotImplementedException();

        public string DriverVersion => throw new NotImplementedException();

        public short InterfaceVersion => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public string SensorDescription(string PropertyName)
        {
            throw new NotImplementedException();
        }

        public double TimeSinceLastUpdate(string PropertyName)
        {
            throw new NotImplementedException();
        }
    }
}
