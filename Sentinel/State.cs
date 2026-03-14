namespace Sentinel
{
    using ASCOM.Common.DeviceInterfaces;
    using System.Text;

    public class State
    {
        #region Variables and initialisers

        private static uint serverTransactionId = 0;

        public State() { }

        #endregion

        #region public properties

        public bool Connected { get; set { field = value; RaiseChangeEvent(nameof(Connected)); } } = false;
        public bool ConnectingToDevices { get; set; } = false;



        public List<SafetyState> LastSafetyState { get; set; } = [];

        /// <summary>
        /// Current width of the view port window
        /// </summary>
        public int WindowWidth { get; set { field = value; RaiseChangeEvent(nameof(WindowWidth)); } } = 1280;

        /// <summary>
        /// Gets or sets the index of the first visible log entry in the log view.
        /// </summary>
        public int TopOfVisibleLog { get; set; } = 0;

        /// <summary>
        /// Gets the number of lines in the screen log when last updated
        /// </summary>
        public int LastNumberOfLogLines { get; set; } = 0;

        public StringBuilder ApplicationLog { get; set; } = new StringBuilder(Globals.MAXIMUM_LOG_SIZE_CHARACTERS, Globals.MAXIMUM_LOG_SIZE_CHARACTERS).Append($"{Globals.WELCOME_MESSAGE}\r\n");

        public List<IObservingConditionsV2> ObservingConditionsDevices { get; set; } = [];

        public List<DiscoveredDevice> DiscoveredObservingConditionsDevices = new List<DiscoveredDevice>();
        public List<DiscoveredDevice> DiscoveredSafetyMonitorDevices = new List<DiscoveredDevice>();
        public bool DiscoveryUnderway { get; set; } = false;
        public bool DiscoveryHasRun { get; set; } = false;

        public Dictionary<PropertyName, IObservingConditionsV2> ObservingConditionsDeviceMap { get; set; } = [];

        public Dictionary<PropertyName, ISafetyMonitorV3> SafetyMonitorDevices { get; set; } = [];


        public Dictionary<int, ISwitchV3> SwitchDevices { get; set; } = [];

        #endregion

        #region Public functions

        public uint GetServerTransactionId()
        {
            serverTransactionId++;
            return serverTransactionId;
        }

        #endregion

        private void RaiseChangeEvent(string memberName)
        {
            if (OnConfigurationChanged is not null)
            {
                EventArgs args = new();
                OnConfigurationChanged(this, args);
            }
        }

        public event EventHandler? OnConfigurationChanged;

        /// <summary>
        /// Set by Index.razor so that other components (e.g. NavMenu) can trigger the connect/disconnect flow.
        /// </summary>
        public Func<Task>? ConnectRequested { get; set; }

    }
}
