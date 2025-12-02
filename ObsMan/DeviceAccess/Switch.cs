using ASCOM.Common.DeviceInterfaces;

namespace ObsMan.DeviceAccess
{
    public class Switch : ISwitchV3
    {
        public bool Connecting => throw new NotImplementedException();

        public List<StateValue> DeviceState => throw new NotImplementedException();

        public short MaxSwitch => throw new NotImplementedException();

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
