using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace ObsMan.DeviceAccess
{
    public class SafetyMonitor : ISafetyMonitorV3
    {
        private readonly Settings settings;
        private readonly State state;
        private readonly ObsManLogger logger;

        PropertyName[] safetyMonitors = [
            PropertyName.SafetyMonitor0,
                PropertyName.SafetyMonitor1,
                PropertyName.SafetyMonitor2,
                PropertyName.SafetyMonitor3,
                PropertyName.SafetyMonitor4,
                PropertyName.SafetyMonitor5,
                PropertyName.SafetyMonitor6,
                PropertyName.SafetyMonitor7,
                PropertyName.SafetyMonitor8,
                PropertyName.SafetyMonitor9];

        public SafetyMonitor(Settings settings, State state, ObsManLogger logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings;
            this.state = state;
            this.logger = logger;
        }
        private record CacheEntry<T>(T Value, Exception? Exception, DateTime Timestamp);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        // Classes to hold cache records and locks for each property to allow concurrent reads of different properties without blocking each other
        private readonly ConcurrentDictionary<PropertyName, CacheEntry<bool>> _propertyCache = new();
        private readonly ConcurrentDictionary<PropertyName, Lock> _propertyLocks = new();

        /// <summary>Reads a device property value, re-throwing NotImplementedException as-is and wrapping all other exceptions in a NotImplementedException.</summary>
        /// <remarks>Results (including exceptions) are cached for <see cref="CacheExpiry"/>. Each property has its own lock so concurrent reads of different properties do not block each other.</remarks>
        private bool GetCachedBool(PropertyName propertyName, Func<bool> getValue)
        {
            Lock propertyLock = _propertyLocks.GetOrAdd(propertyName, _ => new Lock());
            lock (propertyLock)
            {
                // Return the cached result if it exists and the call time is still within the expiry window
                if ((_propertyCache.TryGetValue(propertyName, out CacheEntry<bool>? entry)) && (DateTime.UtcNow - entry.Timestamp < settings.PropertyCacheTime)) // Value is cached and within the expiry time so return the last value
                {
                    if (entry.Exception is null) // Cache hit with a valid value so return the value without calling the device
                        return entry.Value;

                    // Last call returned an exception, and we're still within the cache expiry window, so re-throw the same exception without calling the device again
                    throw entry.Exception;
                }

                // Cache miss or expired — send to the real device
                try
                {
                    bool value = getValue(); // Call the provided delegate to get the property value from the device
                    _propertyCache[propertyName] = new CacheEntry<bool>(value, null, DateTime.UtcNow); // Cache the successful result
                    return value;
                }
                catch (Exception ex) // The device returned an exception — cache and re-throw it
                {
                    _propertyCache[propertyName] = new CacheEntry<bool>(false, ex, DateTime.UtcNow); // Cache the exception result
                    throw;
                }
            }
        }

        public bool IsSafe
        {
            get
            {
                logger.LogMessageConsole("IsSafe", $"Called - Connected: {Connected}");
                if (Connected)
                {
                    state.LastSafetyState.Clear(); // Clear the last safety state list to be populated with any new safety events detected in this call
                    bool allSafe = true;

                    foreach (PropertyName property in safetyMonitors)
                    {
                        if (state.SafetyMonitorDevices.TryGetValue(property, out ISafetyMonitorV3? entry))
                        {
                            bool isSafe = GetCachedBool(property, () => state.SafetyMonitorDevices[property].IsSafe); // All monitors must report safe for overall safety
                            if (!isSafe)
                            {
                                logger.LogMessageConsole("IsSafe", $"Safety monitor {property} reported unsafe.");
                                state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.Unsafe, SafetyEventType.SafetyIssue, property.ToString(), $"Safety monitor {property} reported unsafe.")); // Add a safety event to the list for any monitor that reports unsafe
                                allSafe = false;
                            }
                        }
                    }
                    logger.LogMessageConsole("IsSafe", $"Safety monitor state: {allSafe}");

                    // Exit here if any monitor is not safe, no need to check the observing conditions rules if we're already not safe based on the safety monitors
                    if (!allSafe)
                        return false;

                    // Check whether any rules are set for this property, and if so evaluate them against the current value of the property. If any rule is satisfied then we're not safe.
                    foreach (PropertyName property in Globals.ObservingConditionsProperties)
                    {
                        // Get the four rule elements into local variables for easier reference
                        EqualityType equalityType1 = settings.ObservingCondtionsRules[property].EqualityType1;
                        double value1 = settings.ObservingCondtionsRules[property].Value1;
                        EqualityType equalityType2 = settings.ObservingCondtionsRules[property].EqualityType2;
                        double value2 = settings.ObservingCondtionsRules[property].Value2;

                        // Check whether any rules are set, if not, exit early and consider this property as safe. 
                        if (equalityType1 == EqualityType.NotInUse && equalityType2 == EqualityType.NotInUse)
                            continue;

                        // Get the current value of this property from the observing conditions device if it exists, if not, consider this property as safe and continue to the next property.
                        state.ObservingConditionsDeviceMap.TryGetValue(property, out IObservingConditionsV2? observingConditionsDevice);
                        if (observingConditionsDevice == null)
                            continue;

                        // Get the current value of this property from the observing conditions device
                        double currentValue = 0.0;

                        switch (property)
                        {
                            case PropertyName.CloudCover:
                                currentValue = observingConditionsDevice.CloudCover;
                                break;

                            case PropertyName.DewPoint:
                                currentValue = observingConditionsDevice.DewPoint;
                                break;

                            case PropertyName.Humidity:
                                currentValue = observingConditionsDevice.Humidity;
                                break;
                            case PropertyName.Temperature:
                                currentValue = observingConditionsDevice.Temperature;
                                break;

                            case PropertyName.Pressure:
                                currentValue = observingConditionsDevice.Pressure;
                                break;

                            case PropertyName.RainRate:
                                currentValue = observingConditionsDevice.RainRate;
                                break;

                            case PropertyName.SkyBrightness:
                                currentValue = observingConditionsDevice.SkyBrightness;
                                break;

                            case PropertyName.SkyQuality:
                                currentValue = observingConditionsDevice.SkyQuality;
                                break;

                            case PropertyName.SkyTemperature:
                                currentValue = observingConditionsDevice.SkyTemperature;
                                break;

                            case PropertyName.StarFWHM:
                                currentValue = observingConditionsDevice.StarFWHM;
                                break;

                            case PropertyName.WindDirection:
                                currentValue = observingConditionsDevice.WindDirection;
                                break;

                            case PropertyName.WindSpeed:
                                currentValue = observingConditionsDevice.WindSpeed;
                                break;

                            case PropertyName.WindGust:
                                currentValue = observingConditionsDevice.WindGust;
                                break;

                            default:
                                throw new InvalidOperationException($"Unrecognised property name: {property}");
                        }

                        // Evaluate the equality 1 rules against the current value of the property
                        switch (equalityType1)
                        {
                            case EqualityType.NotInUse: // No rule set for this property so ignore it
                                break;

                            case EqualityType.LessThan:
                                if (currentValue < value1)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions {property} value {currentValue} is less than {value1}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.BelowLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 1 violated: {property} value {currentValue} is less than {value1}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;

                            case EqualityType.Equal:
                                if (currentValue == value1)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions {property} value {currentValue} is equal to {value1}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.EqualLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 1 violated: {property} value {currentValue} is equal to {value1}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;

                            case EqualityType.GreaterThan:
                                if (currentValue > value1)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions {property} value {currentValue} is greater than {value1}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.AboveLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 1 violated: {property} value {currentValue} is greater than {value1}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;
                        }

                        // Evaluate the equality 2 rules against the current value of the property
                        switch (equalityType2)
                        {
                            case EqualityType.NotInUse: // No rule set for this property so ignore it
                                break;

                            case EqualityType.LessThan:
                                if (currentValue < value2)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is less than {value2}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.BelowLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 2 violated: {property} value {currentValue} is less than {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;

                            case EqualityType.Equal:
                                if (currentValue == value2)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is equal {value2}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.EqualLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 2 violated: {property} value {currentValue} is equal to {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;

                            case EqualityType.GreaterThan:
                                if (currentValue > value2)
                                {
                                    logger.LogMessageConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is greater than {value2}.");
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.AboveLimit,
                                        property.ToSafetyEventType(),
                                        $"{settings.ServerName}",
                                        $"Observing conditions rule 2 violated: {property} value {currentValue} is greater than {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                    allSafe = false; // Rule not satisfied, set allSafe to false
                                }
                                break;
                        }
                    }

                    return allSafe;
                }
                throw new ASCOM.NotConnectedException("Observatory Manager safety monitor is not connected.");
            }
        }

        public List<StateValue> DeviceState
        {
            get
            {
                List<StateValue> stateValues = [];

                try { stateValues.Add(new StateValue(nameof(IsSafe), IsSafe)); } catch { }

                return stateValues;
            }
        }

        public string Description => "Observatory Manager - Description";

        public string DriverInfo => "Observatory Manager - Driver Info";

        public string DriverVersion => "0.1";

        public short InterfaceVersion => 3;
        public string Name => "Observatory Manager - Name";

        public IList<string> SupportedActions => [Globals.SAFETY_EVENT_ACTION_NAME];

        public string Action(string actionName, string actionParameters)
        {
            logger.LogMessageConsole("Action", $"Called with name: {actionName}, parameters: {actionParameters}");
            actionName = actionName.Trim().ToLowerInvariant();
            switch (actionName)
            {
                case Globals.SAFETY_EVENT_ACTION_NAME_LOWERCASE:
                    logger.LogMessageConsole("Action", $"Returning JSON string.");
                    //return JsonSerializer.Serialize(state.LastSafetyState);
                    return JsonSerializer.Serialize(state.LastSafetyState, _jsonOptions);
            }

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
    }
}
