using ASCOM;
using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
using Microsoft.OpenApi.Any;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace Sentinel.DeviceAccess
{
    public class SafetyMonitor : ISafetyMonitorV3
    {
        private readonly Settings settings;
        private readonly State state;
        private readonly SentinelLogger logger;


        public SafetyMonitor(Settings settings, State state, SentinelLogger logger)
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

        /// <summary>Reads a device property value, re-throwing <see cref="ASCOM.NotImplementedException"/> as-is and wrapping all other exceptions in a <see cref="ASCOM.NotImplementedException"/>.</summary>
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
                // Check whether remote access is enabled
                CheckEnabled();

                if (Connected)
                {
                    state.LastSafetyState.Clear(); // Clear the last safety state list to be populated with any new safety events detected in this call
                    bool allSafe = true;

                    // Iterate over the safety monitor configurations
                    foreach (PropertyName property in Globals.SafetyMonitorNames)
                    {
                        string safetyMessage = "";

                        // Check whether this Alpaca or COM safety monitor device is available for use
                        if (state.SafetyMonitorDevices.TryGetValue(property, out ISafetyMonitorV3? entry)) // The device 
                        {
                            // Check how we are configured to handle this safety monitor
                            switch (settings.SafetyMonitorSettings[property])
                            {
                                case SafetyMonitorState.Enabled: // The monitor is enabled for normal use so check its value
                                    try
                                    {
                                        if (state.SafetyMonitorDevices[property] is not null) // The device connected OK
                                        {
                                            bool isSafe = GetCachedBool(property, () => state.SafetyMonitorDevices[property].IsSafe);
                                            if (!isSafe)
                                            {
                                                safetyMessage = $"Safety monitor {property} reported an UNSAFE condition.";
                                                logger.LogWarningConsole("IsSafe", safetyMessage);
                                                state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.Unsafe, SafetyEventType.SafetyIssue, $"{Globals.APPLICATION_NAME} at {settings.Location}", safetyMessage)); // Add a safety event to the list for any monitor that reports unsafe
                                                allSafe = false;
                                            }
                                        }
                                        else // The device failed to connect so report an error
                                        {
                                            safetyMessage = $"{settings.ConfiguredDevices[property].DisplayName} ({property}) failed to connect.";
                                            logger.LogErrorConsole("IsSafe", safetyMessage);
                                            state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.DeviceInErrorState, SafetyEventType.SafetyIssue, $"{Globals.APPLICATION_NAME} at {settings.Location}", safetyMessage)); // Add a safety event to the list for any monitor that reports unsafe
                                            allSafe = false;
                                        }
                                    }
                                    catch (Exception ex)  // Any error results in isSafe remaining false, and a safety event being added to the list below
                                    {
                                        safetyMessage = $"Exception getting {property}: {ex.Message}";
                                        logger.LogErrorConsole("IsSafe", safetyMessage);
                                        if (settings.LogLevel <= ASCOM.Common.Interfaces.LogLevel.Debug)
                                            logger.LogMessageConsole("IsSafe", $"Full exception:\r\n{ex}");

                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.DeviceInErrorState, SafetyEventType.SafetyIssue, $"{Globals.APPLICATION_NAME} at {settings.Location}", safetyMessage)); // Add a safety event to the list for any monitor that reports unsafe
                                        allSafe = false;
                                    }
                                    break;

                                case SafetyMonitorState.ForceFalse: // The monitor is configured always to report an UNSAFE condition
                                    // Add a safety event to the list when the response is forced to UNSAFE
                                    safetyMessage = $"Safety monitor {property} is configured to report UNSAFE regardless of the state of the device.";
                                    logger.LogWarningConsole("IsSafe", safetyMessage);
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.ForcedToState, SafetyEventType.SafetyIssue, $"{Globals.APPLICATION_NAME} at {settings.Location}", safetyMessage)); // Add a safety event to the list for any monitor that reports unsafe
                                    allSafe = false;
                                    break;

                                case SafetyMonitorState.ForceTrue: // The monitor is configured always to report a SAFE condition
                                    // Add a safety event to the list when the response is forced to SAFE
                                    safetyMessage = $"Safety monitor {property} is configured to report SAFE regardless of the state of the device.";
                                    logger.LogWarningConsole("IsSafe", safetyMessage);
                                    state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.ForcedToState, SafetyEventType.SafetyIssue, $"{Globals.APPLICATION_NAME} at {settings.Location}", safetyMessage)); // Add a safety event to the list for any monitor that reports unsafe
                                    break;

                                default:
                                    throw new InvalidValueException($"Unknown safety monitor state {settings.SafetyMonitorSettings[property]} for device {property}.");
                            }
                        }
                    }

                    // Check whether any observing conditions rules are set for this property, and if so evaluate them against the current value of the property. If any rule is satisfied then we're not safe.
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
                        try
                        {
                            // Get the property's current value from the device
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
                                    throw new ASCOM.InvalidOperationException($"Unrecognised property name: {property}");
                            }

                            // Evaluate the equality 1 rules against the current value of the property
                            switch (equalityType1)
                            {
                                case EqualityType.NotInUse: // No rule set for this property so ignore it
                                    break;

                                case EqualityType.LessThan:
                                    if (currentValue < value1)
                                    {
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions {property} value {currentValue} is less than {value1}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.BelowLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 1 violated: Value {currentValue.ToRoundedString()} is less than {value1}.")); // Add a safety event to the list for any rule that is not satisfied
                                        allSafe = false; // Rule not satisfied, set allSafe to false
                                    }
                                    break;

                                case EqualityType.Equal:
                                    if (currentValue == value1)
                                    {
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions {property} value {currentValue} is equal to {value1}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.EqualLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 1 violated: Value {currentValue.ToRoundedString()} is equal to {value1}.")); // Add a safety event to the list for any rule that is not satisfied
                                        allSafe = false; // Rule not satisfied, set allSafe to false
                                    }
                                    break;

                                case EqualityType.GreaterThan:
                                    if (currentValue > value1)
                                    {
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions {property} value {currentValue} is greater than {value1}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.AboveLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 1 violated: Value {currentValue.ToRoundedString()} is greater than {value1}.")); // Add a safety event to the list for any rule that is not satisfied
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
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is less than {value2}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.BelowLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is less than {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                        allSafe = false; // Rule not satisfied, set allSafe to false
                                    }
                                    break;

                                case EqualityType.Equal:
                                    if (currentValue == value2)
                                    {
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is equal {value2}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.EqualLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is equal to {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                        allSafe = false; // Rule not satisfied, set allSafe to false
                                    }
                                    break;

                                case EqualityType.GreaterThan:
                                    if (currentValue > value2)
                                    {
                                        logger.LogWarningConsole("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is greater than {value2}.");
                                        state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.AboveLimit,
                                            property.ToSafetyEventType(),
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                            $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is greater than {value2}.")); // Add a safety event to the list for any rule that is not satisfied
                                        allSafe = false; // Rule not satisfied, set allSafe to false
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogErrorConsole("IsSafe", $"Exception getting value for {property} - {ex.Message}.");
                            state.LastSafetyState.Add(new SafetyState(SafetyEventCondition.DeviceInErrorState,
                                property.ToSafetyEventType(),
                                $"{Globals.APPLICATION_NAME} at {settings.Location}",
                                $"Exception getting value for {property} - {ex.Message}.")); // Add a safety event to the list for any rule that is not satisfied
                            allSafe = false; // Rule not satisfied, set allSafe to false
                        }
                    }

                    return allSafe;
                }
                throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
            }
        }

        private void CheckEnabled()
        {
            if (!Connected)
                throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
        }

        public List<StateValue> DeviceState
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled();

                List<StateValue> stateValues = [];

                try { stateValues.Add(new StateValue(nameof(IsSafe), IsSafe)); } catch { }

                return stateValues;
            }
        }

        public string Description => $"{Globals.APPLICATION_NAME} - Aggregates a collection of SafetyMonitor devices into a single composite device.";

        public string DriverInfo => $"{Globals.APPLICATION_NAME} - Version {Globals.APPLICATION_VERSION}";

        public string DriverVersion => Globals.APPLICATION_VERSION;

        public short InterfaceVersion => 3;
        public string Name => $"{Globals.APPLICATION_NAME} - Safety monitor device";

        public IList<string> SupportedActions
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled();
                return [Globals.SAFETY_EVENT_ACTION_NAME];
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            // Check whether remote access is enabled
            CheckEnabled();

            logger.LogDebugConsole("Action", $"Called with name: {actionName}, parameters: {actionParameters}");
            actionName = actionName.Trim().ToLowerInvariant();
            switch (actionName)
            {
                case Globals.SAFETY_EVENT_ACTION_NAME_LOWERCASE:
                    logger.LogDebug("Action", $"Returning JSON string.");
                    return JsonSerializer.Serialize(state.LastSafetyState, _jsonOptions);
            }

            throw new ActionNotImplementedException($"Action not implemented: {actionName}");
        }

        public void CommandBlind(string command, bool raw = false)
        {
            throw new ASCOM.NotImplementedException("CommandBlind is not implemented.");
        }

        public bool CommandBool(string command, bool raw = false)
        {
            throw new ASCOM.NotImplementedException("CommandBool is not implemented.");
        }

        public string CommandString(string command, bool raw = false)
        {
            throw new ASCOM.NotImplementedException("CommandString is not implemented.");
        }

        private bool connected = false;
        private bool connecting = false;

        public bool Connected
        {
            get
            {
                // Check whether remote access is enabled
                CheckEnabled();

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
                await Task.Delay(500);
                Connecting = false;
                Connected = true;
            });
        }

        public void Disconnect()
        {
            // Check whether remote access is enabled
            CheckEnabled();

            Connecting = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                Connecting = false;

                if (!settings.PreventRemoteDisconnects)
                    Connected = false;
            });
        }

        public void Dispose()
        {

        }
    }
}
