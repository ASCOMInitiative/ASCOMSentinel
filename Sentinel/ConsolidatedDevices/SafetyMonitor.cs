using ASCOM;
using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
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

        public List<SafetyState> lastSafetyState = new List<SafetyState>();

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
        private readonly Lock _safetyStateLock = new();

        /// <summary>Reads a device property value, re-throwing <see cref="ASCOM.NotImplementedException"/> as-is and wrapping all other exceptions in a <see cref="ASCOM.NotImplementedException"/>.</summary>
        /// <remarks>Results (including exceptions) are cached for <see cref="CacheExpiry"/>. Each property has its own lock so concurrent reads of different properties do not block each other.</remarks>
        private bool GetCachedBool(PropertyName propertyName, Func<bool> getValue)
        {
            Lock propertyLock = _propertyLocks.GetOrAdd(propertyName, _ => new Lock());
            lock (propertyLock)
            {
                // Return the cached result if it exists and the call time is still within the expiry window
                if ((_propertyCache.TryGetValue(propertyName, out CacheEntry<bool>? entry)) && (entry.Timestamp + settings.PropertyCacheTime > DateTime.UtcNow)) // Value is cached and within the expiry time so return the last value
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
                // Check whether remote access is enabled and the real devices are connected, throw an exception if not
                CheckEnabled();

                // Check whether we're connected to the client, if not, throw an exception. 
                if (Connected)
                {
                    // Return the cached IsSafe value if it's still valid to avoid the need to update it.
                    if ((state.LastIsSafeTime + settings.PropertyCacheTime) > DateTime.UtcNow)
                        return state.LastIsSafeState;

                    // Update the safety state by checking all the safety monitors and observing conditions rules, and return the overall safety state.
                    // This is done inside a lock to prevent concurrent updates to the safety state when multiple clients call IsSafe at the same time
                    lock (_safetyStateLock)
                    {
                        // Check the cached state again in case it was updated by another thread while we were waiting for the lock.
                        if ((state.LastIsSafeTime + settings.PropertyCacheTime) > DateTime.UtcNow)
                            return state.LastIsSafeState;

                        state.LastSafetyState = "[]"; // Clear the last safety state string to be populated with any new safety events detected in this call
                        lastSafetyState.Clear();

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
                                                    LogWarning("IsSafe", safetyMessage);
                                                    lastSafetyState.Add(new SafetyState(
                                                        $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                        $"Safety monitor {property}", // Rule name
                                                        $"{state.InstanceId}_{property}", // Rule ID
                                                        SafetyEventType.SafetyIssue, // Event type
                                                        SafetyEventCondition.Unsafe, // Event condition
                                                        safetyMessage)); // Event message

                                                    allSafe = false;
                                                }
                                            }
                                            else // The device failed to connect so report an error
                                            {
                                                safetyMessage = $"{settings.ConfiguredDevices[property].DisplayName} ({property}) failed to connect.";
                                                LogError("IsSafe", safetyMessage);
                                                lastSafetyState.Add(new SafetyState(
                                                     $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                     $"Safety monitor {property}", // Rule name
                                                     $"{state.InstanceId}_{property}", // Rule ID
                                                     SafetyEventType.SafetyIssue, // Event type
                                                     SafetyEventCondition.DeviceInErrorState, // Event condition
                                                     safetyMessage)); // Event message
                                                allSafe = false;
                                            }
                                        }
                                        catch (Exception ex)  // Any error results in isSafe remaining false, and a safety event being added to the list below
                                        {
                                            safetyMessage = $"Exception getting {property}: {ex.Message}";
                                            logger.LogError("IsSafe", safetyMessage);
                                            logger.LogDebug("IsSafe", $"Full exception:\r\n{ex}");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Safety monitor {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                SafetyEventType.SafetyIssue, // Event type
                                                SafetyEventCondition.DeviceInErrorState, // Event condition
                                                safetyMessage)); // Event message

                                            allSafe = false;
                                        }
                                        break;

                                    case SafetyMonitorState.ForceFalse: // The monitor is configured always to report an UNSAFE condition
                                                                        // Add a safety event to the list when the response is forced to UNSAFE
                                        safetyMessage = $"Safety monitor {property} is configured to report UNSAFE regardless of the state of the device.";
                                        LogWarning("IsSafe", safetyMessage);
                                        lastSafetyState.Add(new SafetyState(
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                            $"Safety monitor {property}", // Rule name
                                            $"{state.InstanceId}_{property}", // Rule ID
                                            SafetyEventType.SafetyIssue, // Event type
                                            SafetyEventCondition.ForcedToState, // Event condition
                                            safetyMessage)); // Event message

                                        allSafe = false;
                                        break;

                                    case SafetyMonitorState.ForceTrue: // The monitor is configured always to report a SAFE condition
                                                                       // Add a safety event to the list when the response is forced to SAFE
                                        safetyMessage = $"Safety monitor {property} is configured to report SAFE regardless of the state of the device.";
                                        LogWarning("IsSafe", safetyMessage);
                                        lastSafetyState.Add(new SafetyState(
                                            $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                            $"Safety monitor {property}", // Rule name
                                            $"{state.InstanceId}_{property}", // Rule ID
                                            SafetyEventType.SafetyIssue, // Event type
                                            SafetyEventCondition.ForcedToState, // Event condition
                                            safetyMessage)); // Event message

                                        allSafe = false;
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
                                            LogWarning("IsSafe", $"Observing conditions {property} value {currentValue} is less than {value1}.");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Observing conditions {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                property.ToSafetyEventType(), // Event type
                                                SafetyEventCondition.BelowLimit, // Event condition
                                                $"{property} rule 1 violated: Value {currentValue.ToRoundedString()} is less than {value1}.")); // Add a safety event to the list for any rule that is not satisfied

                                            allSafe = false; // Rule not satisfied, set allSafe to false
                                        }
                                        break;

                                    case EqualityType.Equal:
                                        if (currentValue == value1)
                                        {
                                            LogWarning("IsSafe", $"Observing conditions {property} value {currentValue} is equal to {value1}.");

                                            lastSafetyState.Add(new SafetyState(
                                               $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                               $"Observing conditions {property}", // Rule name
                                               $"{state.InstanceId}_{property}", // Rule ID
                                               property.ToSafetyEventType(), // Event type
                                               SafetyEventCondition.EqualLimit, // Event condition
                                               $"{property} rule 1 violated: Value {currentValue.ToRoundedString()} is equal to {value1}.")); // Add a safety event to the list for any rule that is not satisfied

                                            allSafe = false; // Rule not satisfied, set allSafe to false
                                        }
                                        break;

                                    case EqualityType.GreaterThan:
                                        if (currentValue > value1)
                                        {
                                            LogWarning("IsSafe", $"Observing conditions {property} value {currentValue} is greater than {value1}.");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Observing conditions {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                property.ToSafetyEventType(), // Event type
                                                SafetyEventCondition.AboveLimit, // Event condition
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
                                            LogWarning("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is less than {value2}.");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Observing conditions {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                property.ToSafetyEventType(), // Event type
                                                SafetyEventCondition.BelowLimit, // Event condition
                                                $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is less than {value2}.")); // Add a safety event to the list for any rule that is not satisfied

                                            allSafe = false; // Rule not satisfied, set allSafe to false
                                        }
                                        break;

                                    case EqualityType.Equal:
                                        if (currentValue == value2)
                                        {
                                            LogWarning("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is equal {value2}.");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Observing conditions {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                property.ToSafetyEventType(), // Event type
                                                SafetyEventCondition.EqualLimit, // Event condition
                                                $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is equal to {value2}.")); // Add a safety event to the list for any rule that is not satisfied

                                            allSafe = false; // Rule not satisfied, set allSafe to false
                                        }
                                        break;

                                    case EqualityType.GreaterThan:
                                        if (currentValue > value2)
                                        {
                                            LogWarning("IsSafe", $"Observing conditions rule 2 violated: {property} value {currentValue} is greater than {value2}.");

                                            lastSafetyState.Add(new SafetyState(
                                                $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                                $"Observing conditions {property}", // Rule name
                                                $"{state.InstanceId}_{property}", // Rule ID
                                                property.ToSafetyEventType(), // Event type
                                                SafetyEventCondition.AboveLimit, // Event condition
                                                $"{property} rule 2 violated: Value {currentValue.ToRoundedString()} is greater than {value2}.")); // Add a safety event to the list for any rule that is not satisfied

                                            allSafe = false; // Rule not satisfied, set allSafe to false
                                        }
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError("IsSafe", $"Exception getting value for {property} - {ex.Message}.");

                                lastSafetyState.Add(new SafetyState(
                                    $"{Globals.APPLICATION_NAME} at {settings.Location}", // Event source
                                    $"Observing conditions {property}", // Rule name
                                    $"{state.InstanceId}_{property}", // Rule ID
                                    property.ToSafetyEventType(), // Event type
                                    SafetyEventCondition.DeviceInErrorState, // Event condition
                                    $"Exception getting value for {property} - {ex.Message}.")); // Add a safety event to the list for any rule that is not satisfied

                                allSafe = false; // Rule not satisfied, set allSafe to false
                            }
                        }

                        // Update the cached safety state
                        state.LastSafetyState = JsonSerializer.Serialize(lastSafetyState, _jsonOptions);

                        // Update the time of the last safety state update
                        state.LastIsSafeTime = DateTime.UtcNow;

                        // Return the overall safety state
                        return allSafe;
                    } // lock
                }
                throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
            }
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

        public string Description => $"{Globals.APPLICATION_NAME} - Aggregates several SafetyMonitor devices into a single composite device and reports on safety state.";

        public string DriverInfo => $"{Globals.APPLICATION_NAME} - Version {state.InformationalVersion}";

        public string DriverVersion
        {
            get
            {
                string[] parts = state.ApplicationFileversion.Split('.');
                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : state.ApplicationFileversion;
            }
        }

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

            logger.LogDebug("Action", $"Action method called: {actionName}, parameters: {actionParameters}");
            actionName = actionName.Trim().ToLowerInvariant();
            switch (actionName)
            {
                case Globals.SAFETY_EVENT_ACTION_NAME_LOWERCASE:
                    // Update the state.LastSafetyState value by calling IsSafe
                    _ = IsSafe;

                    // Now return the current safety state 
                    lock (_safetyStateLock) // Get the lock to prevent other threads from modifying the value while we read it
                    {
                        logger.LogDebug("Action", $"Returning JSON string: {state.LastSafetyState}");

                        return state.LastSafetyState;
                    }
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
                    logger.LogError("SafetyMonitor.Connect", $"Exception: {ex.Message}");
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
                    logger.LogError("SafetyMonitor.Disconnect", $"Exception: {ex.Message}");
                    Connecting = false;
                }
            });
        }

        public void Dispose()
        {

        }

        #region Support code

        private void LogWarning(string method, string message)
        {
            if (settings.LogSafetyWarnings)
                logger.LogWarning(method, message);
        }

        private void LogError(string method, string message)
        {
            if (settings.LogSafetyWarnings)
                logger.LogError(method, message);
        }

        private void CheckEnabled()
        {
            // Check whether the application is online
            if (!state.Online)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is offline.");

            // Check whether the real devices are connected
            if (!state.Connected)
                throw new ASCOM.InvalidOperationException($"{Globals.APPLICATION_NAME} is not connected to it's real devices.");

            // Check whether we are connected
            //if (!connected)
            //    throw new ASCOM.NotConnectedException($"{Globals.APPLICATION_NAME} safety monitor is not connected.");
        }

        #endregion

    }
}
