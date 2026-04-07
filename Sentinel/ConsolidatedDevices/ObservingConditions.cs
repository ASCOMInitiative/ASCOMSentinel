using ASCOM.Common.DeviceInterfaces;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Sentinel.DeviceAccess
{
    public class ObservingConditions : IObservingConditionsV2
    {
        private readonly Settings settings;
        private readonly State state;
        private readonly SentinelLogger logger;

        // Record defining a cache entry for double and string results (e.g. property values and SensorDescription values)
        private record CacheEntry<T>(T Value, Exception? Exception, DateTime Timestamp);

        // Classes to hold cache records and locks for each property to allow concurrent reads of different properties without blocking each other
        private readonly ConcurrentDictionary<PropertyName, CacheEntry<double>> _propertyCache = new();
        private readonly ConcurrentDictionary<PropertyName, Lock> _propertyLocks = new();

        // Cache and locks for SensorDescription results
        private readonly ConcurrentDictionary<PropertyName, CacheEntry<string>> _sensorDescriptionCache = new();
        private readonly ConcurrentDictionary<PropertyName, Lock> _sensorDescriptionLocks = new();

        private Lock deviceStateLock = new Lock();

        public ObservingConditions(Settings settings, State state, SentinelLogger logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.state = state;
            this.logger = logger;
        }

        private void CheckEnabled()
        {
            // Check whether the application is online
            if (!state.Online)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is offline.");

            // Check whether the real devices are connected
            if (!state.Connected)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is not connected to it's real devices.");

            //if (!connected)
            //    throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
        }
        private void CheckEnabled(PropertyName propertyName)
        {
            // Check whether the application is online
            if (!state.Online)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is offline.");

            // Check whether the real devices are connected
            if (!state.Connected)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is not connected to it's real devices.");

            if (!state.ObservingConditionsDeviceMap.ContainsKey(propertyName))
                throw new ASCOM.NotImplementedException($"{propertyName} is not implemented in this observing conditions device.");


            //if (!connected)
            //    throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
        }

        public List<StateValue> DeviceState
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled();

                // Return the cached result if it exists and the call time is still within the expiry window
                if ((state.LastObservingConditionsDeviceStateTime + settings.PropertyCacheTime) > DateTime.UtcNow)
                    return state.LastObservingConditionsDeviceState;

                lock (deviceStateLock)
                {
                    // Repeat the cache test in case another thread has already updated the cache while we were waiting for the lock
                    if ((state.LastObservingConditionsDeviceStateTime + settings.PropertyCacheTime) > DateTime.UtcNow)
                        return state.LastObservingConditionsDeviceState;

                    state.LastObservingConditionsDeviceState = [];

                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(CloudCover), CloudCover)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(DewPoint), DewPoint)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(Humidity), Humidity)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(Pressure), Pressure)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(RainRate), RainRate)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(SkyBrightness), SkyBrightness)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(SkyQuality), SkyQuality)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(SkyTemperature), SkyTemperature)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(StarFWHM), StarFWHM)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(Temperature), Temperature)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(WindDirection), WindDirection)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(WindSpeed), WindSpeed)); } catch { }
                    try { state.LastObservingConditionsDeviceState.Add(new StateValue(nameof(WindGust), WindGust)); } catch { }

                    state.LastObservingConditionsDeviceStateTime = DateTime.UtcNow;
                    return state.LastObservingConditionsDeviceState;
                }
            }
        }

        public double AveragePeriod
        {
            get => 0.0; set
            {
                // Check whether remote access is enabled
                CheckEnabled();

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

                if ((_propertyCache.TryGetValue(propertyName, out CacheEntry<double>? entry)) && (entry.Timestamp + settings.PropertyCacheTime > DateTime.UtcNow)) // Value is cached and within the expiry time so return the last value
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
                if ((_sensorDescriptionCache.TryGetValue(propertyName, out CacheEntry<string>? entry)) && (entry.Timestamp + settings.PropertyCacheTime > DateTime.UtcNow))
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
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.CloudCover);

                if (state.ObservingConditionsDeviceMap[PropertyName.CloudCover] == null)
                    throw new ASCOM.NotImplementedException($"CloudCover is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.CloudCover, () => state.ObservingConditionsDeviceMap[PropertyName.CloudCover].CloudCover);
            }
        }

        public double DewPoint
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.DewPoint);

                return GetCachedDouble(PropertyName.DewPoint, () => state.ObservingConditionsDeviceMap[PropertyName.DewPoint].DewPoint);
            }
        }

        public double Humidity
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.Humidity);

                if (state.ObservingConditionsDeviceMap[PropertyName.Humidity] == null)
                    throw new ASCOM.NotImplementedException($"Humidity is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Humidity, () => state.ObservingConditionsDeviceMap[PropertyName.Humidity].Humidity);
            }
        }

        public double Pressure
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.Pressure);

                if (state.ObservingConditionsDeviceMap[PropertyName.Pressure] == null)
                    throw new ASCOM.NotImplementedException($"Pressure is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Pressure, () => state.ObservingConditionsDeviceMap[PropertyName.Pressure].Pressure);
            }
        }

        public double RainRate
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.RainRate);

                if (state.ObservingConditionsDeviceMap[PropertyName.RainRate] == null)
                    throw new ASCOM.NotImplementedException($"RainRate is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.RainRate, () => state.ObservingConditionsDeviceMap[PropertyName.RainRate].RainRate);
            }
        }

        public double SkyBrightness
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.SkyBrightness);

                if (state.ObservingConditionsDeviceMap[PropertyName.SkyBrightness] == null)
                    throw new ASCOM.NotImplementedException($"SkyBrightness is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyBrightness, () => state.ObservingConditionsDeviceMap[PropertyName.SkyBrightness].SkyBrightness);
            }
        }

        public double SkyQuality
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.SkyQuality);

                if (state.ObservingConditionsDeviceMap[PropertyName.SkyQuality] == null)
                    throw new ASCOM.NotImplementedException($"SkyQuality is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyQuality, () => state.ObservingConditionsDeviceMap[PropertyName.SkyQuality].SkyQuality);
            }
        }

        public double StarFWHM
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.StarFWHM);

                if (state.ObservingConditionsDeviceMap[PropertyName.StarFWHM] == null)
                    throw new ASCOM.NotImplementedException($"StarFWHM is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.StarFWHM, () => state.ObservingConditionsDeviceMap[PropertyName.StarFWHM].StarFWHM);
            }
        }

        public double SkyTemperature
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.SkyTemperature);

                if (state.ObservingConditionsDeviceMap[PropertyName.SkyTemperature] == null)
                    throw new ASCOM.NotImplementedException($"SkyTemperature is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.SkyTemperature, () => state.ObservingConditionsDeviceMap[PropertyName.SkyTemperature].SkyTemperature);
            }
        }
        public double Temperature
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.Temperature);

                if (state.ObservingConditionsDeviceMap[PropertyName.Temperature] == null)
                    throw new ASCOM.NotImplementedException($"Temperature is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.Temperature, () => state.ObservingConditionsDeviceMap[PropertyName.Temperature].Temperature);
            }
        }

        public double WindDirection
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.WindDirection);

                if (state.ObservingConditionsDeviceMap[PropertyName.WindDirection] == null)
                    throw new ASCOM.NotImplementedException($"WindDirection is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindDirection, () => state.ObservingConditionsDeviceMap[PropertyName.WindDirection].WindDirection);
            }
        }

        public double WindGust
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.WindGust);

                if (state.ObservingConditionsDeviceMap[PropertyName.WindGust] == null)
                    throw new ASCOM.NotImplementedException($"WindGust is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindGust, () => state.ObservingConditionsDeviceMap[PropertyName.WindGust].WindGust);
            }
        }

        public double WindSpeed
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled(PropertyName.WindSpeed);

                if (state.ObservingConditionsDeviceMap[PropertyName.WindSpeed] == null)
                    throw new ASCOM.NotImplementedException($"WindSpeed is not implemented in this observing conditions device.");

                return GetCachedDouble(PropertyName.WindSpeed, () => state.ObservingConditionsDeviceMap[PropertyName.WindSpeed].WindSpeed);
            }
        }

        public string Description => $"{Globals.APPLICATION_NAME} - Aggregates several ObservingConditions devices into a single composite device.";

        public string DriverInfo => $"{Globals.APPLICATION_NAME} - Version {state.InformationalVersion}";

        public string DriverVersion
        {
            get
            {
                string[] parts = state.ApplicationFileversion.Split('.');
                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : state.ApplicationFileversion;
            }
        }
        public short InterfaceVersion => 2;
        public string Name => $"{Globals.APPLICATION_NAME} - Observing Conditions device";

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

        public bool Connected
        {
            get
            {
                return connected;
            }

            set
            {
                // Check whether remote access is enabled
                CheckEnabled();

                connected = value;
            }
        }

        public bool Connecting
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled();

                return connecting;
            }

            set
            {
                // Check whether remote access is enabled
                CheckEnabled();

                connecting = value;
            }
        }

        public void Connect()
        {
            // Check whether remote access is enabled
            CheckEnabled();

            Connecting = true;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    Connecting = false;
                    Connected = true;
                }
                catch (Exception ex)
                {
                    logger.LogError("ObservingConditions.Connect", $"Exception: {ex.Message}");
                    Connecting = false;
                }
            });
        }

        public void Disconnect()
        {
            // Check whether remote access is enabled
            CheckEnabled();

            Connecting = true;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    Connecting = false;

                    if (!settings.PreventRemoteDisconnects)
                        Connected = false;
                }
                catch (Exception ex)
                {
                    logger.LogError("ObservingConditions.Disconnect", $"Exception: {ex.Message}");
                    Connecting = false;
                }
            });
        }

        public void Dispose()
        {

        }

        public void Refresh()
        {
            // Check whether remote access is enabled
            CheckEnabled();
        }

        public string SensorDescription(string PropertyName)
        {
            // Check whether remote access is enabled
            CheckEnabled();

            PropertyName? propertyEnum = ToPropertyName(PropertyName);
            if (!propertyEnum.HasValue)
                throw new ASCOM.InvalidValueException($"Property name '{PropertyName}' is not a valid ObservingConditions property name.");

            if (!state.ObservingConditionsDeviceMap.ContainsKey(propertyEnum.Value))
                throw new ASCOM.NotImplementedException($"{PropertyName} sensor description is not available because the device is not configured.");

            return GetCachedString(propertyEnum.Value, () => state.ObservingConditionsDeviceMap[propertyEnum.Value].SensorDescription(PropertyName));
        }

        public double TimeSinceLastUpdate(string PropertyName)
        {
            // Check whether remote access is enabled
            CheckEnabled();

            if (string.IsNullOrEmpty(PropertyName))
                return 1;

            PropertyName? propertyEnum = ToPropertyName(PropertyName);
            if (!propertyEnum.HasValue)
                throw new ASCOM.InvalidValueException($"Property name '{PropertyName}' is not a valid ObservingConditions property name.");

            if (!state.ObservingConditionsDeviceMap.ContainsKey(propertyEnum.Value))
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
