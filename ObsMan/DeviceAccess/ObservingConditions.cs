using ASCOM.Common.DeviceInterfaces;
using System.Reflection.Metadata.Ecma335;

namespace ObsMan.DeviceAccess
{
    public class ObservingConditions : IObservingConditionsV2
    {
        private readonly State state;
        private readonly ObsManLogger logger;

        public ObservingConditions(State state, ObsManLogger logger)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(logger);
            this.state = state;
            this.logger = logger;
        }

        public List<StateValue> DeviceState
        {
            get
            {
                List<StateValue> stateValues = new List<StateValue>();
                try { stateValues.Add(new StateValue(nameof(CloudCover), CloudCover)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(DewPoint), DewPoint)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(Humidity), Humidity)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(Pressure), Pressure)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(RainRate), RainRate)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(SkyBrightness), SkyBrightness)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(SkyQuality), SkyQuality)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(SkyTemperature), SkyTemperature)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(StarFWHM), StarFWHM)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(Temperature), Temperature)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(WindDirection), WindDirection)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(WindSpeed), WindSpeed)); } catch (Exception ex) { Console.WriteLine(ex); }
                try { stateValues.Add(new StateValue(nameof(WindGust), WindGust)); } catch (Exception ex) { Console.WriteLine(ex); }

                return stateValues;
            }
        }

        double averagePeriod = 0.0;
        public double AveragePeriod
        {
            get => 0.0; set
            {
                if (value < 0.0)
                    throw new ASCOM.InvalidValueException($"{value} is not a valid average period.");
            }
        }

        /// <summary>Reads a device property value, re-throwing NotImplementedException as-is and wrapping all other exceptions in a NotImplementedException.</summary>
        private double GetPropertyValue(Func<double> getValue)
        {
            try
            {
                return getValue();
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }

        public double CloudCover => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.CloudCover].CloudCover);

        public double DewPoint => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.DewPoint].DewPoint);

        public double Humidity => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.Humidity].Humidity);

        public double Pressure => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.Pressure].Pressure);

        public double RainRate => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.RainRate].RainRate);

        public double SkyBrightness => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.SkyBrightness].SkyBrightness);

        public double SkyQuality => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.SkyQuality].SkyQuality);

        public double StarFWHM => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.StarFWHM].StarFWHM);

        public double SkyTemperature => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.SkyTemperature].SkyTemperature);

        public double Temperature => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.Temperature].Temperature);

        public double WindDirection => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.WindDirection].WindDirection);

        public double WindGust => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.WindGust].WindGust);

        public double WindSpeed => GetPropertyValue(() => state.ObservingConditionsDeviceMap[PropertyName.WindSpeed].WindSpeed);

        public string Description => "Observatory Manager - Description";

        public string DriverInfo => "Observatory Manager - Driver Info";

        public string DriverVersion => "0.1";

        public short InterfaceVersion => 2;
        public string Name => "Observatory Manager - Name";

        public IList<string> SupportedActions => new List<string>();

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

        private bool connected = false;
        private bool connecting = false;

        public bool Connected { get => connected; set => connected = value; }

        public bool Connecting { get => connecting; set => connecting = value; }

        public void Connect()
        {
            Connecting = true;
            Task.Run(() =>
            {
                Thread.Sleep(500);
                Connecting = false;
                Connected = true;
            });
        }

        public void Disconnect()
        {
            Connecting = true;
            Task.Run(() =>
            {
                Thread.Sleep(500);
                Connecting = false;
                Connected = false;
            });
        }

        public void Dispose()
        {

        }

        public void Refresh()
        {

        }

        public string SensorDescription(string PropertyName)
        {
            PropertyName? propertyEnum = ToPropertyName(PropertyName);
            if (propertyEnum.HasValue)
                return state.ObservingConditionsDeviceMap[propertyEnum.Value].SensorDescription(PropertyName);

            throw new ASCOM.InvalidValueException($"Property name '{PropertyName}' is not a valid ObservingConditions property name.");
        }

        public double TimeSinceLastUpdate(string PropertyName)
        {
            return 1;
        }

        /// <summary>
        /// Returns the <see cref="PropertyName"/> enum value that matches the supplied string, using a case-insensitive comparison.
        /// Returns <c>null</c> if the string does not match any enum member.
        /// </summary>
        public static PropertyName? ToPropertyName(string name)
        {
            if (Enum.TryParse<PropertyName>(name, ignoreCase: true, out PropertyName result))
                return result;

            return null;
        }
    }
}
