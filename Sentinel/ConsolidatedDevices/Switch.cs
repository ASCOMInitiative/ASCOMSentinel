using ASCOM.Common.DeviceInterfaces;

namespace Sentinel.DeviceAccess
{
    public class Switch : ISwitchV3
    {
        private readonly Settings settings;
        private readonly State state;
        private readonly AppLogger logger;

        public Switch(Settings settings, State state, AppLogger logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.state = state;
            this.logger = logger;
        }

        public bool Connecting => throw new NotImplementedException();

        public List<StateValue> DeviceState => throw new NotImplementedException();

        public short MaxSwitch => throw new NotImplementedException();

        public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Description => $"{Globals.APPLICATION_NAME} - Aggregates a collection of Switch devices into a single composite device.";

        public string DriverInfo => $"{Globals.APPLICATION_NAME} - Version {state.InformationalVersion}";

        public string DriverVersion
        {
            get
            {
                string[] parts = state.ApplicationFileversion.Split('.');
                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : state.ApplicationFileversion;
            }
        }

        public short InterfaceVersion => throw new NotImplementedException();

        public string Name => $"{Globals.APPLICATION_NAME} - Switch device";

        public IList<string> SupportedActions => throw new NotImplementedException();

        public string Action(string actionName, string actionParameters)
        {
            throw new NotImplementedException();
        }

        public bool CanAsync(short id)
        {
            throw new NotImplementedException();
        }

        public void CancelAsync(short id)
        {
            throw new NotImplementedException();
        }

        public bool CanWrite(short id)
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

        public bool GetSwitch(short id)
        {
            throw new NotImplementedException();
        }

        public string GetSwitchDescription(short id)
        {
            throw new NotImplementedException();
        }

        public string GetSwitchName(short id)
        {
            throw new NotImplementedException();
        }

        public double GetSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        public double MaxSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        public double MinSwitchValue(short id)
        {
            throw new NotImplementedException();
        }

        public void SetAsync(short id, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetAsyncValue(short id, double value)
        {
            throw new NotImplementedException();
        }

        public void SetSwitch(short id, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetSwitchName(short id, string name)
        {
            throw new NotImplementedException();
        }

        public void SetSwitchValue(short id, double value)
        {
            throw new NotImplementedException();
        }

        public bool StateChangeComplete(short id)
        {
            throw new NotImplementedException();
        }

        public double SwitchStep(short id)
        {
            throw new NotImplementedException();
        }
    }
}
