using ASCOM.Common.DeviceInterfaces;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ObsMan.DeviceAccess
{
    public class ObservingConditions : IObservingConditionsV2
    {
        private readonly Settings settings;
        private readonly State state;
        private readonly ObsManLogger logger;

        // Record defining a cache entry for double and string results (e.g. property values and SensorDescription values)
        private record CacheEntry<T>(T Value, Exception? Exception, DateTime Timestamp);

        // Classes to hold cache records and locks for each property to allow concurrent reads of different properties without blocking each other
        private readonly ConcurrentDictionary<PropertyName, CacheEntry<double>> _propertyCache = new();
        private readonly ConcurrentDictionary<PropertyName, Lock> _propertyLocks = new();

        // Cache and locks for SensorDescription results
        private readonly ConcurrentDictionary<PropertyName, CacheEntry<string>> _sensorDescriptionCache = new();
        private readonly ConcurrentDictionary<PropertyName, Lock> _sensorDescriptionLocks = new();

        public ObservingConditions(Settings settings, State state, ObsManLogger logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.state = state;
            this.logger = logger;
        }

        public List<StateValue> DeviceState
        {
            get
            {
                List<StateValue> stateValues = [];

                try { stateValues.Add(new StateValue(nameof(CloudCover), CloudCover)); } catch { }
                try { stateValues.Add(new StateValue(nameof(DewPoint), DewPoint)); } catch { }
                try { stateValues.Add(new StateValue(nameof(Humidity), Humidity)); } catch { }
                try { stateValues.Add(new StateValue(nameof(Pressure), Pressure)); } catch { }
                try { stateValues.Add(new StateValue(nameof(RainRate), RainRate)); } catch { }
                try { stateValues.Add(new StateValue(nameof(SkyBrightness), SkyBrightness)); } catch { }
                try { stateValues.Add(new StateValue(nameof(SkyQuality), SkyQuality)); } catch { }
                try { stateValues.Add(new StateValue(nameof(SkyTemperature), SkyTemperature)); } catch { }
                try { stateValues.Add(new StateValue(nameof(StarFWHM), StarFWHM)); } catch { }
                try { stateValues.Add(new StateValue(nameof(Temperature), Temperature)); } catch { }
                try { stateValues.Add(new StateValue(nameof(WindDirection), WindDirection)); } catch { }
                try { stateValues.Add(new StateValue(nameof(WindSpeed), WindSpeed)); } catch { }
                try { stateValues.Add(new StateValue(nameof(WindGust), WindGust)); } catch { }

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
        /// <remarks>Results (including exceptions) are cached for <see cref="CacheExpiry"/>. Each property has its own lock so concurrent reads of different properties do not block each other.</remarks>
        private double GetCachedDouble(PropertyName propertyName, Func<double> getValue)
        {
            Lock propertyLock = _propertyLocks.GetOrAdd(propertyName, _ => new Lock());
            lock (propertyLock)
            {
                // Return the cached result if it exists and the call time is still within the expiry window
                if ((_propertyCache.TryGetValue(propertyName, out CacheEntry<double>? entry)) && (DateTime.UtcNow - entry.Timestamp < settings.PropertyCacheTime)) // Value is cached and within the expiry time so return the last value
                {
                    if (entry.Exception is null) // Cache hit with a valid value so return the value without calling the device
                        return entry.Value;

                    // Last call returned an exception, and we're still within the cache expiry window, so re-throw the same exception without calling the device again
                    throw entry.Exception;
                }

                // Cache miss or expired — send to the real device
                try
                {
                    double value = getValue(); // Call the provided delegate to get the property value from the device
                    _propertyCache[propertyName] = new CacheEntry<double>(value, null, DateTime.UtcNow); // Cache the successful result
                    return value;
                }
                catch (Exception ex) // The device returned an exception — cache and re-throw it
                {
                    _propertyCache[propertyName] = new CacheEntry<double>(0, ex, DateTime.UtcNow); // Cache the exception result
                    throw;
                }
            }
        }

        /// <summary>Reads a string result from a device method (e.g. SensorDescription), caching both successful results and exceptions.</summary>
        /// <remarks>Results (including exceptions) are cached for the configured cache time. Each property has its own lock.</remarks>
        private string GetCachedString(PropertyName propertyName, Func<string> getValue)
        {
            Lock propertyLock = _sensorDescriptionLocks.GetOrAdd(propertyName, _ => new Lock());
            lock (propertyLock)
            {
                // Return the cached result if it exists and the call time is still within the expiry window
                if ((_sensorDescriptionCache.TryGetValue(propertyName, out CacheEntry<string>? entry)) && (DateTime.UtcNow - entry.Timestamp < settings.PropertyCacheTime))
                {
                    if (entry.Exception is null)
                        return entry.Value;
                    throw entry.Exception;
                }

                // Cache miss or expired — call the real method
                try
                {
                    string value = getValue();
                    _sensorDescriptionCache[propertyName] = new CacheEntry<string>(value, null, DateTime.UtcNow);
                    return value;
                }
                catch (Exception ex)
                {
                    _sensorDescriptionCache[propertyName] = new CacheEntry<string>(string.Empty, ex, DateTime.UtcNow);
                    throw;
                }
            }
        }

        public double CloudCover
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.CloudCover]  == null)
                    throw new ASCOM.NotImplementedException($"CloudCover is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.CloudCover, () => state.ObservingConditionsDeviceMap[PropertyName.CloudCover].CloudCover);
            }
        }
        public double DewPoint
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.DewPoint] == null)
                    throw new ASCOM.NotImplementedException($"DewPoint is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.DewPoint, () => state.ObservingConditionsDeviceMap[PropertyName.DewPoint].DewPoint);
            }
        }

        public double Humidity
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.Humidity] == null)
                    throw new ASCOM.NotImplementedException($"Humidity is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Humidity, () => state.ObservingConditionsDeviceMap[PropertyName.Humidity].Humidity);
            }
        }

        public double Pressure
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.Pressure] == null)
                    throw new ASCOM.NotImplementedException($"Pressure is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Pressure, () => state.ObservingConditionsDeviceMap[PropertyName.Pressure].Pressure);
            }
        }

        public double RainRate
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.RainRate] == null)
                    throw new ASCOM.NotImplementedException($"RainRate is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.RainRate, () => state.ObservingConditionsDeviceMap[PropertyName.RainRate].RainRate);
            }
        }

        public double SkyBrightness
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.SkyBrightness] == null)
                    throw new ASCOM.NotImplementedException($"SkyBrightness is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyBrightness, () => state.ObservingConditionsDeviceMap[PropertyName.SkyBrightness].SkyBrightness);
            }
        }

        public double SkyQuality
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.SkyQuality] == null)
                    throw new ASCOM.NotImplementedException($"SkyQuality is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyQuality, () => state.ObservingConditionsDeviceMap[PropertyName.SkyQuality].SkyQuality);
            }
        }

        public double StarFWHM
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.StarFWHM] == null)
                    throw new ASCOM.NotImplementedException($"StarFWHM is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.StarFWHM, () => state.ObservingConditionsDeviceMap[PropertyName.StarFWHM].StarFWHM);
            }
        }

        public double SkyTemperature
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.SkyTemperature] == null)
                    throw new ASCOM.NotImplementedException($"SkyTemperature is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyTemperature, () => state.ObservingConditionsDeviceMap[PropertyName.SkyTemperature].SkyTemperature);
            }
        }
        public double Temperature
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.Temperature] == null)
                    throw new ASCOM.NotImplementedException($"Temperature is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Temperature, () => state.ObservingConditionsDeviceMap[PropertyName.Temperature].Temperature);
            }
        }

        public double WindDirection
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.WindDirection] == null)
                    throw new ASCOM.NotImplementedException($"WindDirection is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindDirection, () => state.ObservingConditionsDeviceMap[PropertyName.WindDirection].WindDirection);
            }
        }

        public double WindGust
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.WindGust] == null)
                    throw new ASCOM.NotImplementedException($"WindGust is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindGust, () => state.ObservingConditionsDeviceMap[PropertyName.WindGust].WindGust);
            }
        }

        public double WindSpeed
        {
            get
            {
                if (state.ObservingConditionsDeviceMap[PropertyName.WindSpeed] == null)
                    throw new ASCOM.NotImplementedException($"WindSpeed is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindSpeed, () => state.ObservingConditionsDeviceMap[PropertyName.WindSpeed].WindSpeed);
            }
        }

        public string Description => "Observatory Manager - Description";

        public string DriverInfo => "Observatory Manager - Driver Info";

        public string DriverVersion => "0.1";

        public short InterfaceVersion => 2;
        public string Name => "Observatory Manager - Name";

        public IList<string> SupportedActions => [];

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
            Task.Run(async () =>
            {
                await Task.Delay(500);
                Connecting = false;
                Connected = true;
            });
        }

        public void Disconnect()
        {
            Connecting = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
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
            if (!propertyEnum.HasValue)
                throw new ASCOM.InvalidValueException($"Property name '{PropertyName}' is not a valid ObservingConditions property name.");

            if (state.ObservingConditionsDeviceMap[propertyEnum.Value] == null)
                throw new ASCOM.NotImplementedException($"{PropertyName} sensor description is not available because the device is not configured.");

            return GetCachedString(propertyEnum.Value, () => state.ObservingConditionsDeviceMap[propertyEnum.Value].SensorDescription(PropertyName));
        }

        public double TimeSinceLastUpdate(string PropertyName)
        {
            if (string.IsNullOrEmpty(PropertyName))
                return 1;

            PropertyName? propertyEnum = ToPropertyName(PropertyName);
            if (!propertyEnum.HasValue)
                throw new ASCOM.InvalidValueException($"Property name '{PropertyName}' is not a valid ObservingConditions property name.");

            if (state.ObservingConditionsDeviceMap[propertyEnum.Value] == null)
                throw new ASCOM.NotImplementedException($"{PropertyName} sensor description is not available because the device is not configured.");

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
