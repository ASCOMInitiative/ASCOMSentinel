namespace Sentinel
{
    using ASCOM.Common.DeviceInterfaces;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Text;

    public class State
    {
        #region Variables and initialisers

        private static uint serverTransactionId;

        public State()
        {
            ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Not set";
            ApplicationFileversion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Not set";
            InformationalVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Not set";
        }

        #endregion

        #region Static methods

        public static void SortDiscoveredDevices(State state, SortByDevice sortbyDevice)
        {
            if ((sortbyDevice == SortByDevice.ObservingConditions) || (sortbyDevice == SortByDevice.All))
            {
                state.DiscoveredObservingConditionsDevices.Sort((a, b) =>
                {
                    int diff = DeviceSortOrder(a).CompareTo(DeviceSortOrder(b));
                    return diff != 0 ? diff : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            }

            if ((sortbyDevice == SortByDevice.SafetyMonitor) || (sortbyDevice == SortByDevice.All))
            {
                state.DiscoveredSafetyMonitorDevices.Sort((a, b) =>
                {
                    int diff = DeviceSortOrder(a).CompareTo(DeviceSortOrder(b));
                    return diff != 0 ? diff : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            }
        }

        private static int DeviceSortOrder(DiscoveredDevice d)
        {
            return d.Protocol switch
            {
                Protocol.NotConfigured => 1,
                Protocol.Alpaca => d.SentinelDeviceType switch
                {
                    SentinelDeviceType.ManualObservingConditions => 2,
                    SentinelDeviceType.ManualSafetyMonitor => 3,
                    SentinelDeviceType.ObservingConditions => 4,
                    SentinelDeviceType.SafetyMonitor => 5,
                    _ => 6,
                },
                Protocol.COM => 7,
                _ => 8,
            };
        }

        #endregion

        #region public properties

        public int GaugeDimension { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(GaugeDimension)); } } }

        public string ApplicationVersion { get; set; } = "Not set";
        public string ApplicationFileversion { get; set; } = "Not set";

        public string InformationalVersion { get; set; } = "Not set";

        public bool Online { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(Online)); } } } = false;

        public bool Connected { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(Connected)); } } } = false;

        public bool DisplayReconnectMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(DisplayReconnectMessage)); } } } = false;

        public bool DisplayRestartMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(DisplayRestartMessage)); } } } = false;

        /// <summary>
        /// Status text displayed on the Setup page (e.g. "must be re-started", "discovery underway").
        /// Stored here so other components (e.g. NavMenu) can clear it.
        /// </summary>
        public string StatusText { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(StatusText)); } } } = "";

        /// <summary>
        /// Set to true by the Index page after its first render completes. NavMenu disables action buttons until this is true.
        /// </summary>
        // public bool UiReady { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(UiReady)); } } } = false;

        public bool OperationUnderway { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(OperationUnderway)); } } } = false;

        /// <summary>
        /// Message displayed to all browsers when an operation is underway (e.g. restart, shutdown).
        /// Stored in shared state so every connected circuit shows the same message.
        /// </summary>
        public string OperationUnderwayMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(OperationUnderwayMessage)); } } } = "Operation Underway";

        public bool ConnectingToDevices { get; set; } = false;

        public StringBuilder ApplicationLog { get; set; } = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");

        public List<IObservingConditionsV2> ObservingConditionsDevices { get; set; } = [];

        public List<DiscoveredDevice> DiscoveredObservingConditionsDevices = new List<DiscoveredDevice>();

        public List<DiscoveredDevice> DiscoveredSafetyMonitorDevices = new List<DiscoveredDevice>();

        /// <summary>
        /// Lock protecting <see cref="DiscoveredObservingConditionsDevices"/> and <see cref="DiscoveredSafetyMonitorDevices"/> from concurrent mutation.
        /// </summary>
        public Lock DiscoveredDevicesLock { get; } = new();

        public List<SafetyState> LastSafetyState { get; set; } = [];

        /// <summary>
        /// Captured at startup from Settings.RequireAdministratorLogin.
        /// Changes to the setting do not take effect until the application is restarted.
        /// </summary>
        public bool RequireAdministratorLoginAtStartup { get; set; } = true;

        public bool DiscoveryHasRun { get; set; } = false;

        public ConcurrentDictionary<PropertyName, IObservingConditionsV2> ObservingConditionsDeviceMap { get; set; } = new();

        public ConcurrentDictionary<PropertyName, ISafetyMonitorV3> SafetyMonitorDevices { get; set; } = new();

        public ConcurrentDictionary<int, ISwitchV3> SwitchDevices { get; set; } = new();

        public IObservingConditionsV2 ObservingConditions { get; set; } = null!;

        public ISafetyMonitorV3 SafetyMonitor { get; set; } = null!;

        #endregion

        #region Public functions

        public uint GetServerTransactionId()
        {
            return Interlocked.Increment(ref serverTransactionId);
        }

        /// <summary>
        /// Resets all runtime state properties to their initial default values.
        /// Existing <see cref="StateChanged"/> subscribers are preserved.
        /// Assembly-derived properties (<see cref="ApplicationVersion"/> etc.) and
        /// <see cref="DiscoveredDevicesLock"/> are not modified.
        /// </summary>
        public void ResetState()
        {
            GaugeDimension = Globals.GAUGE_DIMENSION_DEFAULT;
            Online = false;
            Connected = false;
            DisplayReconnectMessage = false;
            DisplayRestartMessage = false;
            StatusText = string.Empty;
            OperationUnderway = false;
            OperationUnderwayMessage = "Operation Underway";
            ConnectingToDevices = false;
            ApplicationLog = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS)
                .Append($"{Globals.WELCOME_MESSAGE}\r\n");
            ObservingConditionsDevices = [];
            lock (DiscoveredDevicesLock)
            {
                DiscoveredObservingConditionsDevices = new List<DiscoveredDevice>();
                DiscoveredSafetyMonitorDevices = new List<DiscoveredDevice>();
            }
            LastSafetyState = [];
            RequireAdministratorLoginAtStartup = true;
            DiscoveryHasRun = false;
            ObservingConditionsDeviceMap = new ConcurrentDictionary<PropertyName, IObservingConditionsV2>();
            SafetyMonitorDevices = new ConcurrentDictionary<PropertyName, ISafetyMonitorV3>();
            SwitchDevices = new ConcurrentDictionary<int, ISwitchV3>();
            ObservingConditions = null!;
            SafetyMonitor = null!;
        }

        #endregion

        #region Event handlers

        public void RaiseChangeEvent(string memberName)
        {
            try
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                // Prevent a failing subscriber from breaking other callers
            }
        }

        public event EventHandler? StateChanged;

        #endregion
    }
}
