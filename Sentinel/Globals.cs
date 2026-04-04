namespace Sentinel
{
    internal static class Globals
    {
        internal const string APPLICATION_SHORT_NAME = "Sentinel";
        internal const string APPLICATION_NAME = $"ASCOM {APPLICATION_SHORT_NAME}";
        internal const string OBSERVING_CONDITIONS_NAME = $"{APPLICATION_NAME} Observing Conditions";
        internal const string SAFETY_MONITOR_NAME = $"{APPLICATION_NAME} Safety Monitor";

        internal const int MESSAGE_LEVEL_WIDTH = 8; // Width to which the message level will be padded
        internal const int TEST_NAME_WIDTH = 35; // Width allowed for test names in screen display and log files
        internal const string APPLICATION_FOLDER_NAME = @"ASCOM\Sentinel"; // Application folder name underneath the local application data folder
        internal const string SETTINGS_FILENAME = "sentinel.settings"; // Settings file name
        internal const string LOG_FILENAME = "sentinel.log"; // Log file name
        internal const string WELCOME_MESSAGE = $"Welcome to {APPLICATION_NAME}!"; // Welcome message
        internal const string MANUFACTURER_NAME = "Peter Simpson"; // Manufacturer name 
        internal const int DEFAULT_ALPACA_PORT = 32324; // Default Alpaca port
        internal const int MAXIMUM_LOG_SIZE_CHARACTERS = 60000; // Maximum log file size in characters before truncation
        internal const int LOG_TRUNCATION_CHARACTERS = 6000; // The number of characters by which to truncate the screen log when required.
        internal const bool ALPACA_SEARCH_STRICT_CASING = false;
        internal const string USER_AGENT_PRODUCT_NAME = "ObservatoryManager";

        // Suppressible messages
        internal const string DISCOVERY_PACKET_MESSAGE = "Received a discovery packet from"; // Message that is logged when a discovery packet is received, which can be suppressed in the settings.

        internal const string SAFETY_EVENT_ACTION_NAME = "GetSafetyState";
        internal const string SAFETY_EVENT_ACTION_NAME_LOWERCASE = "getsafetystate"; // Lowercase version of the action name for comparison when strict casing is disabled

        internal const string NOT_SET = "Not configured";

        internal static readonly Lock writeLogLock = new(); // Lock object to synchronize access to the log when resizing

        internal const int APPLICATION_SHUTDOWN_TIMEOUT = 5; // Time that the application waits for host services to stop after a shutdown request before forcing termination (Default: 30 seconds)

        internal const int WEBSOCKET_CLOSE_TIMEOUT = 5; // Time the web-socket transport waits for a graceful close (Default: 5 seconds)
        internal const int RESTART_DELAY = 1; // Time to wait before restarting the application after a restart request. (Seconds)

        internal const int DISCONNECTED_CIRCUIT_RETENTION_PERIOD = 180; // Maximum time that circuit state is retained on the server before being cleaned out (Default 180 seconds)

        internal const int LOG_REFRESH_INTERVAL = 250; // Interval at which the log is refreshed on the screen (Milliseconds)

        internal const int GAUGE_DIMENSION_DEFAULT = 350; // Default gauge dimension in pixels
        internal const int GAUGE_SMALL_TRANSTION = 260; // Gauge dimension in pixels at which the gauge transitions to the small layout.
        internal const int GAUGE_SMALL_OFFSET = 54; // Offset in pixels applied to the position of the gauge value text when the gauge is in the small layout to prevent overlap with the gauge arc.


        internal static readonly SemaphoreSlim ConnectSemaphore = new SemaphoreSlim(1, 1);
        internal static Lock StateLock = new();

        internal static List<PropertyName> ObservingConditionsProperties = new()
        {
            PropertyName.CloudCover,
            PropertyName.DewPoint,
            PropertyName.Humidity,
            PropertyName.Pressure,
            PropertyName.RainRate,
            PropertyName.SkyBrightness,
            PropertyName.SkyQuality,
            PropertyName.SkyTemperature,
            PropertyName.StarFWHM,
            PropertyName.Temperature,
            PropertyName.WindDirection,
            PropertyName.WindGust,
            PropertyName.WindSpeed
        };

        internal static List<PropertyName> SafetyMonitorNames = new()
        {
            PropertyName.SafetyMonitor0,
            PropertyName.SafetyMonitor1,
            PropertyName.SafetyMonitor2,
            PropertyName.SafetyMonitor3,
            PropertyName.SafetyMonitor4,
            PropertyName.SafetyMonitor5,
            PropertyName.SafetyMonitor6,
            PropertyName.SafetyMonitor7,
            PropertyName.SafetyMonitor8,
            PropertyName.SafetyMonitor9
        };

        internal static List<string> SuppressableMessages = new() // List of messages that can be suppressed by SentinelLogger.
        {
            DISCOVERY_PACKET_MESSAGE
        }; 
    }
}
