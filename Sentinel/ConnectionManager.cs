using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
using System.Diagnostics;

namespace Sentinel
{
    internal static class ConnectionManager
    {
        /// <summary>
        /// Handles the connect/disconnect button click, toggling device connection state.
        /// When <paramref name="connectOnly"/> is true (used for auto-connect on startup),
        /// the method will not disconnect if already connected — preventing multiple browser
        /// tabs from racing to connect and inadvertently toggling the state.
        /// </summary>
        internal static async Task ChangeConnectedStateAsync(State state, Settings settings, AppLogger logger, Func<Task> invokeStateHasChanged, bool connectOnly = false)
        {
            lock (Globals.StateLock)
            {
                // Atomic check-and-set: only one caller wins; all others return immediately.
                if (state.ConnectingToDevices) return;

                // When auto-connecting, skip if another tab already completed the connection.
                if (connectOnly && state.Connected) return;

                state.ConnectingToDevices = true;
            }
            try
            {
                await invokeStateHasChanged();

                if (state.Connected)
                {
                    Disconnect(state, logger);
                    await invokeStateHasChanged();
                }
                else
                {
                    await ConnectAsync(state, settings, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("ChangeConnectedStateAsync", $"Overall exception: \r\n{ex}");
            }
            finally
            {
                state.ConnectingToDevices = false;
                await invokeStateHasChanged();
            }
        }

        /// <summary>
        /// Disconnects all ObservingConditions
        /// </summary>
        internal static void Disconnect(State state, AppLogger logger)
        {
            logger.LogMessage("Disconnect", $"Disconnecting from devices...");

            try
            {
                // Disconnect ObservingConditions devices.
                if (state.ObservingConditionsDevices.Count > 0)
                {
                    logger.LogDebug("Disconnect", $"Disconnecting from ObservingConditions devices");
                    foreach (IObservingConditionsV2 device in state.ObservingConditionsDevices)
                    {
                        try { device.Dispose(); } catch { }
                    }
                    state.ObservingConditionsDevices.Clear();
                    state.ObservingConditionsDeviceMap.Clear();
                }

                // Disconnect SafetyMonitor devices.
                if (state.SafetyMonitorDevices.Count > 0)
                {
                    logger.LogDebug("Disconnect", $"Disconnecting from SafetyMonitor devices");
                    foreach (KeyValuePair<PropertyName, ISafetyMonitorV3> device in state.SafetyMonitorDevices)
                    {
                        try { device.Value?.Dispose(); } catch { }
                    }
                    state.SafetyMonitorDevices.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Disconnect", $"Exception during disconnect: {ex.Message}");
            }
            finally
            {
                logger.LogMessage("Disconnect", $"All devices disconnected");
                logger.LogBlankLine();

                state.Connected = false;
            }
        }

        /// <summary>
        /// Connects to all configured ObservingConditions and SafetyMonitor devices.
        /// </summary>
        internal static async Task ConnectAsync(State state, Settings settings, AppLogger logger)
        {
            await Globals.ConnectSemaphore.WaitAsync();
            try
            {
                // Get a list of unique observing conditions devices to which to connect. we only need one instance even if that device is used for multiple properties.
                List<DiscoveredDevice> uniqueObservingConditionsDevices = settings.ConfiguredDevices.Values
                    .Where(d => (d.SentinelDeviceType == SentinelDeviceType.ObservingConditions) || (d.SentinelDeviceType == SentinelDeviceType.ManualObservingConditions))
                    .DistinctBy(d => (d.DisplayName, d.SentinelDeviceType, d.ComProgID, d.IpAddress, d.PortNumber, d.RemoteDeviceNumber)).ToList();

                // Log a warning if there are duplicate entries where the DisplayName values differ — same DisplayName means intentional sharing of one device across multiple properties.
                foreach (var group in settings.ConfiguredDevices
                    .Where(d => d.Value.SentinelDeviceType == SentinelDeviceType.ObservingConditions ||
                                d.Value.SentinelDeviceType == SentinelDeviceType.ManualObservingConditions)
                    .GroupBy(d => (d.Value.ComProgID, d.Value.IpAddress, d.Value.PortNumber, d.Value.RemoteDeviceNumber))
                    .Where(g => g.Count() > 1 && g.Select(d => d.Value.DisplayName).Distinct().Count() > 1))
                {
                    logger.LogWarning("Connect", $"Duplicate Observing Conditions device configured {group.Count()} times: " +
                        $"IpAddress={group.Key.IpAddress}, Port={group.Key.PortNumber}, " +
                        $"DeviceNumber={group.Key.RemoteDeviceNumber}, ComProgID='{group.Key.ComProgID}'");
                    foreach (var entry in group)
                        logger.LogWarning("Connect", $"  -> Property: {entry.Key,-14} - Device: {entry.Value.DisplayName}");
                }

                // Get a list of safety monitor devices to which to connect.
                Dictionary<PropertyName, DiscoveredDevice> safetyMonitorDevices = settings.ConfiguredDevices
                    .Where(d => (d.Value.SentinelDeviceType == SentinelDeviceType.SafetyMonitor) || (d.Value.SentinelDeviceType == SentinelDeviceType.ManualSafetyMonitor)).ToDictionary(d => d.Key, d => d.Value);

                // Log a warning if there are duplicate entries where the DisplayName values differ — same DisplayName means intentional sharing of one device across multiple properties.
                foreach (var group in settings.ConfiguredDevices
                    .Where(d => d.Value.SentinelDeviceType == SentinelDeviceType.SafetyMonitor ||
                                d.Value.SentinelDeviceType == SentinelDeviceType.ManualSafetyMonitor)
                    .GroupBy(d => (d.Value.ComProgID, d.Value.IpAddress, d.Value.PortNumber, d.Value.RemoteDeviceNumber))
                    .Where(g => g.Count() > 1))
                {
                    logger.LogWarning("Connect", $"Duplicate Safety Monitor device configured {group.Count()} times: " +
                        $"IpAddress={group.Key.IpAddress}, Port={group.Key.PortNumber}, " +
                        $"DeviceNumber={group.Key.RemoteDeviceNumber}, ComProgID='{group.Key.ComProgID}'");
                    foreach (var entry in group)
                        logger.LogWarning("Connect", $"  -> Device: {entry.Key,-14} - Device: {entry.Value.DisplayName}");
                }

                // Get counts for re-use multiple times
                int observingConditionsCount = uniqueObservingConditionsDevices.Count();
                int safetyMonitorCount = safetyMonitorDevices.Count();

                // Bail out here if no devices are configured.
                if (observingConditionsCount == 0 && safetyMonitorCount == 0)
                {
                    logger.LogMessage("Connect", $"Cannot connect to devices because none are configured.");
                    return;
                }

                // Connect to configured devices.
                try
                {
                    logger.LogMessage("Connect", $"Connecting to {observingConditionsCount} observing conditions device{(observingConditionsCount == 1 ? "" : "s")} and {safetyMonitorCount} safety monitor device{(safetyMonitorCount == 1 ? "" : "s")}...");

                    // Define a dictionary to hold the unique device instances
                    Dictionary<DiscoveredDevice, IObservingConditionsV2> observingConditionsDeviceInstances = new();

                    // Disconnect from any currently connected devices before connecting to the new set of devices.
                    if (state.Connected)
                        Disconnect(state, logger);

                    List<Task> startTasks = []; // List of tasks to set Connected=true on each device

                    // Iterate over the unique observing conditions devices, create a client instance and connect task for each one add it to the dictionary.
                    foreach (DiscoveredDevice device in uniqueObservingConditionsDevices)
                    {
                        // Create a COM or Alpaca client as appropriate for the device
                        switch (device.Protocol)
                        {
                            case Protocol.Alpaca: // Alpaca so create an Alpaca client
                                IObservingConditionsV2 alpacaDevice = new ASCOM.Alpaca.Clients.AlpacaObservingConditions(new ASCOM.Alpaca.Clients.AlpacaConfiguration()
                                {
                                    ServiceType = ASCOM.Common.Alpaca.ServiceType.Http,
                                    IpAddressString = device.IpAddress,
                                    PortNumber = device.PortNumber,
                                    RemoteDeviceNumber = device.RemoteDeviceNumber,
                                    EstablishConnectionTimeout = settings.AlpacaConnectTimeout, // Seconds
                                    StandardDeviceResponseTimeout = settings.AlpacaGetPropertyTimeout, // Seconds
                                    Logger = settings.IncludeAlpacaTrace ? logger : null,
                                    UserAgentProductName = "Sentinel",
                                    UserAgentProductVersion = "0.1",
                                    ClientNumber = 10 + (uint)uniqueObservingConditionsDevices.IndexOf(device),
                                    NumberOfRetries = 0
                                });

                                observingConditionsDeviceInstances[device] = alpacaDevice;
                                state.ObservingConditionsDevices.Add(alpacaDevice);
                                logger.LogDebug("Connect", $"Connect: added a real Alpaca ObservingConditions device for {device.DisplayName}");
                                break;

#if WINDOWS
                            case Protocol.COM: // COM so create a DriverAccess COM client
                                if (OperatingSystem.IsWindows())
                                {
                                    IObservingConditionsV2 comDevice = new ASCOM.Com.DriverAccess.ObservingConditions(device.ComProgID);
                                    observingConditionsDeviceInstances[device] = comDevice;
                                    state.ObservingConditionsDevices.Add(comDevice);
                                    logger.LogDebug("Connect", $"Connect: added a real COM ObservingConditions device for {device.DisplayName}");
                                }
                                break;
#endif
                            case Protocol.NotConfigured:
                                // Do nothing here, report a message later.
                                break;

                            default:
                                throw new InvalidOperationException($"ConnectionManager.ConnectAsync - Invalid protocol: {device.Protocol}");
                        }

                        // Create a task to set Connected=true for this device. The inner Task.Run wraps the blocking ASCOM call;
                        // a CancellationTokenSource enforces the timeout. On timeout, disposal is deferred until the
                        // orphaned connect task completes to avoid ObjectDisposedException while the ASCOM call is in flight.
                        Task task = Task.Run(async () =>
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            bool connectSucceeded = false;
                            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(settings.AlpacaGetPropertyTimeout));
                            Task connectTask = Task.Run(() =>
                            {
                                try
                                {
                                    // Check whether this device is configured
                                    if (device.Protocol == Protocol.NotConfigured) // Not configured so ignore
                                        logger.LogError("Connect", $"Ignoring un-configured ObservingConditions device - check configuration!");
                                    else // Configured so try to connect
                                    {
                                        logger.LogDebug("Connect", $"Setting Connected True for {device.DisplayName} {device.Protocol}");
                                        observingConditionsDeviceInstances[device].Connected = true;
                                        connectSucceeded = true;
                                        logger.LogMessage("Connect", $"Connected set True OK for {device.DisplayName} {device.Protocol}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (settings.LogLevel == ASCOM.Common.Interfaces.LogLevel.Debug)
                                        logger.LogError("Connect", $"Exception setting Connected true for {device.DisplayName} {device.Protocol} - {ex.Message}\r\n{ex}");
                                    else
                                        logger.LogError("Connect", $"Unable to set Connected true for {device.DisplayName} - {ex.Message}");

                                    // Set a flag that the instance from the device map should be considered invalid
                                    connectSucceeded = false;
                                }
                            });

                            // Wait for the connect to succeed or the timeout to expire.
                            bool timedOut = false;
                            try
                            {
                                await connectTask.WaitAsync(timeoutCts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                timedOut = true;
                                logger.LogDebug("Connect", $"Faulted: {connectTask.IsFaulted} Cancelled: {connectTask.IsCanceled} Completed: {connectTask.IsCompleted} Completed Successfully{connectTask.IsCompletedSuccessfully} Status: {connectTask.Status}");
                                logger.LogError("Connect", $"Timeout ({sw.Elapsed.TotalSeconds:0.0}s) connecting to {device.DisplayName}.");

                                // Null out the reference so it won't be used in the device map, then defer
                                // disposal until the orphaned connect task finishes to avoid ObjectDisposedException.
                                var orphanedDevice = observingConditionsDeviceInstances[device];
                                observingConditionsDeviceInstances[device] = null!;
                                _ = connectTask.ContinueWith(_ =>
                                {
                                    try
                                    {
                                        orphanedDevice?.Dispose();
                                    }
                                    catch { }
                                }, TaskScheduler.Default);
                            }

                            // If the connect did not succeed for any reason, dispose of the device instance and set it to null so that it is not used later on when we try to read properties from it.
                            if (!timedOut && !connectSucceeded)
                            {
                                try
                                {
                                    observingConditionsDeviceInstances[device]?.Dispose();
                                    observingConditionsDeviceInstances[device] = null!;
                                }
                                catch { }
                                //logger.LogError("Connect", $"Failed to connect to {device.DisplayName}.");
                            }
                        });

                        // Add the task to the list of tasks.
                        startTasks.Add(task);
                    }

                    // Create SafetyMonitor device client instances and connect tasks
                    foreach (KeyValuePair<PropertyName, DiscoveredDevice> device in safetyMonitorDevices)
                    {
                        switch (device.Value.Protocol)
                        {
                            case Protocol.Alpaca: // Alpaca so create an Alpaca client
                                ISafetyMonitorV3 alpacaDevice = new ASCOM.Alpaca.Clients.AlpacaSafetyMonitor(new ASCOM.Alpaca.Clients.AlpacaConfiguration()
                                {
                                    ServiceType = ASCOM.Common.Alpaca.ServiceType.Http,
                                    IpAddressString = device.Value.IpAddress,
                                    PortNumber = device.Value.PortNumber,
                                    RemoteDeviceNumber = device.Value.RemoteDeviceNumber,
                                    EstablishConnectionTimeout = settings.AlpacaGetPropertyTimeout, //Seconds
                                    StandardDeviceResponseTimeout = settings.AlpacaGetPropertyTimeout, // Seconds
                                    Logger = settings.IncludeAlpacaTrace ? logger : null,
                                    UserAgentProductName = "Sentinel",
                                    UserAgentProductVersion = "0.1",
                                    ClientNumber = device.Key.ToDeviceNumber(),
                                    NumberOfRetries = 0
                                });

                                state.SafetyMonitorDevices[device.Key] = alpacaDevice;
                                logger.LogDebug("Connect", $"Connect: added a real Alpaca SafetyMonitor device for {device.Value.DisplayName}");
                                break;

#if WINDOWS
                            case Protocol.COM: // COM so create a DriverAccess COM client
                                if (OperatingSystem.IsWindows())
                                {
                                    ISafetyMonitorV3 comDevice = new ASCOM.Com.DriverAccess.SafetyMonitor(device.Value.ComProgID);
                                    state.SafetyMonitorDevices[device.Key] = comDevice;
                                    logger.LogDebug("Connect", $"Connect: added a real COM SafetyMonitor device for {device.Value.DisplayName}");
                                }
                                break;
#endif

                            case Protocol.NotConfigured:
                                // Ignore un-configured manual devices here and give message later.
                                break;

                            default:
                                throw new InvalidOperationException($"ConnectionManager.ConnectAsync - Invalid protocol: {device.Value.Protocol}");
                        }

                        // Connect the safety monitor device with a timeout. Disposal is deferred until the
                        // orphaned connect task completes to avoid ObjectDisposedException while the ASCOM call is in flight.
                        Task task = Task.Run(async () =>
                        {
                            bool connectSucceeded = false;
                            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(settings.AlpacaConnectTimeout + 2.0));
                            Task connectTask = Task.Run(() =>
                            {
                                try
                                {
                                    // Check whether this device is configured
                                    if (device.Value.Protocol == Protocol.NotConfigured) // Not configured so ignore
                                        logger.LogError("Connect", $"Ignoring un-configured SafetyMonitor device - check configuration!");
                                    else // Configured so try to connect
                                    {
                                        logger.LogDebug("Connect", $"Setting Connected True for {device.Value.DisplayName} {device.Value.Protocol}");
                                        state.SafetyMonitorDevices[device.Key].Connected = true;
                                        connectSucceeded = true;
                                        logger.LogMessage("Connect", $"Connected set True OK for {device.Key} {device.Value.DisplayName} {device.Value.Protocol}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    connectSucceeded = false;

                                    if (settings.LogLevel == ASCOM.Common.Interfaces.LogLevel.Debug)
                                        logger.LogError("Connect", $"Exception setting Connected true for {device.Value.DisplayName} {device.Value.Protocol} - {ex.Message}\r\n{ex}");
                                    else
                                        logger.LogError("Connect", $"Unable to set Connected true for {device.Value.DisplayName} - {ex.Message}");
                                }
                            });

                            // Wait for the connect to succeed or the timeout to expire.
                            bool timedOut = false;
                            try
                            {
                                await connectTask.WaitAsync(timeoutCts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                logger.LogDebug("Connect", $"Faulted: {connectTask.IsFaulted} Cancelled: {connectTask.IsCanceled} Completed: {connectTask.IsCompleted} Completed Successfully{connectTask.IsCompletedSuccessfully} Status: {connectTask.Status}");
                                timedOut = true;
                                logger.LogError("Connect", $"Timeout ({settings.AlpacaConnectTimeout}s) connecting to a safety monitor: {device.Value.DisplayName}");

                                // Null out the reference so it won't be used later, then defer disposal
                                // until the orphaned connect task finishes.
                                var orphanedDevice = state.SafetyMonitorDevices[device.Key];
                                state.SafetyMonitorDevices[device.Key] = null!;
                                _ = connectTask.ContinueWith(_ =>
                                {
                                    try { orphanedDevice?.Dispose(); } catch { }
                                }, TaskScheduler.Default);
                            }

                            // If the connect did not succeed and didn't time out, clean up the device instance.
                            if (!timedOut && !connectSucceeded)
                            {
                                try
                                {
                                    state.SafetyMonitorDevices[device.Key]?.Dispose();
                                    state.SafetyMonitorDevices[device.Key] = null!;
                                }
                                catch { }
                            }
                        });
                        startTasks.Add(task);
                    }

                    // Wait for all tasks to finish.
                    await Task.WhenAll(startTasks);
                    logger.LogBlankLine();

                    // Clear the mapping between ObservingConditions properties and devices, then re-populate it based on the configured device definitions and the instances we just created.
                    state.ObservingConditionsDeviceMap.Clear();

                    // Iterate over each property and find the matching device instance based on the configured device for that property
                    foreach (PropertyName property in Globals.ObservingConditionsProperties)
                    {
                        // Get the discovered device information for this property, ignoring devices that are not configured
                        DiscoveredDevice configured = settings.ConfiguredDevices[property];
                        //logger.LogMessage("", $"processing property: {property}, device type: {configured.SentinelDeviceType}");
                        if ((configured.SentinelDeviceType == SentinelDeviceType.ObservingConditions) || (configured.SentinelDeviceType == SentinelDeviceType.ManualObservingConditions))
                        {
                            try
                            {
                                // Find the matching device instance based on the configured device for this property
                                KeyValuePair<DiscoveredDevice, IObservingConditionsV2> device = observingConditionsDeviceInstances.FirstOrDefault(d =>
                                    d.Key.DisplayName == configured.DisplayName &&
                                    d.Key.Protocol == configured.Protocol &&
                                    d.Key.SentinelDeviceType == configured.SentinelDeviceType &&
                                    d.Key.IpAddress == configured.IpAddress &&
                                    d.Key.PortNumber == configured.PortNumber &&
                                    d.Key.RemoteDeviceNumber == configured.RemoteDeviceNumber &&
                                    d.Key.ComProgID == configured.ComProgID);

                                // Check whether a device was returned
                                if (device.Value is not null) // A device was returned so add a map value.
                                {
                                    // Add the matching device instance to the device map so that we can easily find the device for each property later when we need to read values from it.
                                    bool addOutcome = state.ObservingConditionsDeviceMap.TryAdd(property, device.Value);
                                    logger.LogMessage("Connect", $"ObservingConditions.{property,-14} is connected to {device.Key.DisplayName} {device.Key.Protocol}");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError("Connect", $"Exception {property}: \r\n{ex}");
                            }
                        }
                        else
                            logger.LogMessage("Connect", $"ObservingConditions.{property,-14} is not functional.");
                    }
                    logger.LogMessage("Connect", "");
                }
                catch (Exception ex)
                {
                    logger.LogError("Connect", $"Exception during device connection: \r\n{ex}");
                }
                finally
                {
                    state.DisplayReconnectMessage = false;

                    // Only report connected if at least one device connected successfully
                    bool anyObservingConditionsConnected = state.ObservingConditionsDeviceMap.Count > 0;
                    bool anySafetyMonitorConnected = state.SafetyMonitorDevices.Values.Any(d => d is not null);
                    state.Connected = anyObservingConditionsConnected || anySafetyMonitorConnected;

                    if (!state.Connected)
                        logger.LogError("Connect", "No devices connected successfully.");
                }

            }
            finally
            {
                Globals.ConnectSemaphore.Release();
            }
        }
    }
}
