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

        public bool EnableRemoteClients { get; set { field = value; RaiseChangeEvent(nameof(EnableRemoteClients)); } } = false;

        public bool Connected { get; set { field = value; RaiseChangeEvent(nameof(Connected)); } } = false;

        public bool DisplayReconnectMessage { get; set { field = value; RaiseChangeEvent(nameof(DisplayReconnectMessage)); } } = false;

        public bool DisplayRestartMessage { get; set { field = value; RaiseChangeEvent(nameof(DisplayRestartMessage)); } } = false;

        /// <summary>
        /// Status text displayed on the Setup page (e.g. "must be re-started", "discovery underway").
        /// Stored here so other components (e.g. NavMenu) can clear it.
        /// </summary>
        public string StatusText { get; set { field = value; RaiseChangeEvent(nameof(StatusText)); } } = "";

        public bool OperationUnderway { get; set { field = value; RaiseChangeEvent(nameof(OperationUnderway)); } } = false;

        public bool ConnectingToDevices { get; set; } = false;

        public StringBuilder ApplicationLog { get; set; } = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");

        public List<IObservingConditionsV2> ObservingConditionsDevices { get; set; } = [];

        public List<DiscoveredDevice> DiscoveredObservingConditionsDevices = new List<DiscoveredDevice>();

        public List<DiscoveredDevice> DiscoveredSafetyMonitorDevices = new List<DiscoveredDevice>();

        public List<SafetyState> LastSafetyState { get; set; } = [];

        public bool DiscoveryHasRun { get; set; } = false;

        public Dictionary<PropertyName, IObservingConditionsV2> ObservingConditionsDeviceMap { get; set; } = [];

        public Dictionary<PropertyName, ISafetyMonitorV3> SafetyMonitorDevices { get; set; } = [];

        public Dictionary<int, ISwitchV3> SwitchDevices { get; set; } = [];

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
