namespace Sentinel
{
    using ASCOM.Common.DeviceInterfaces;
    using System.Text;

    public class State
    {
        #region Variables and initialisers

        private static uint serverTransactionId;

        public State() { }

        #endregion

        #region public properties

        public bool EnableRemoteClients { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(EnableRemoteClients)); } } } = false;

        public bool Connected { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(Connected)); } } } = false;

        public bool DisplayReconnectMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(DisplayReconnectMessage)); } } } = false;

        public bool DisplayRestartMessage { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(DisplayRestartMessage)); } } } = false;

        /// <summary>
        /// Status text displayed on the Setup page (e.g. "must be re-started", "discovery underway").
        /// Stored here so other components (e.g. NavMenu) can clear it.
        /// </summary>
        public string StatusText { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(StatusText)); } } } = "";

        /// <summary>
        /// Set to true by the Index page after its first render completes.
        /// NavMenu disables action buttons until this is true.
        /// </summary>
        public bool UiReady { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(UiReady)); } } } = false;

        public bool OperationUnderway { get; set { if (field != value) { field = value; RaiseChangeEvent(nameof(OperationUnderway)); } } } = false;

        public bool ConnectingToDevices { get; set; } = false;

        public StringBuilder ApplicationLog { get; set; } = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");

        public List<IObservingConditionsV2> ObservingConditionsDevices { get; set; } = [];

        public List<DiscoveredDevice> DiscoveredObservingConditionsDevices = new List<DiscoveredDevice>();

        public List<DiscoveredDevice> DiscoveredSafetyMonitorDevices = new List<DiscoveredDevice>();

        public List<SafetyState> LastSafetyState { get; set; } = [];

        /// <summary>
        /// Captured at startup from Settings.RequireAdministratorLogin.
        /// Changes to the setting do not take effect until the application is restarted.
        /// </summary>
        public bool RequireAdministratorLoginAtStartup { get; set; } = true;

        public bool DiscoveryHasRun { get; set; } = false;

        public Dictionary<PropertyName, IObservingConditionsV2> ObservingConditionsDeviceMap { get; set; } = [];

        public Dictionary<PropertyName, ISafetyMonitorV3> SafetyMonitorDevices { get; set; } = [];

        public Dictionary<int, ISwitchV3> SwitchDevices { get; set; } = [];

        public IObservingConditionsV2 ObservingConditions { get; set; } = null!;

        public ISafetyMonitorV3 SafetyMonitor { get; set; } = null!;

        #endregion

        #region Public functions

        public uint GetServerTransactionId()
        {
            return Interlocked.Increment(ref serverTransactionId);
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
