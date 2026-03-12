namespace Sentinel
{
    internal static class Globals
    {
        internal const string APPLICATION_NAME = "ASCOM Sentinel";
        internal const string APPLICATION_VERSION = "0.1";
        internal const string MANUFACTURER_VERSION = "0.0.1"; // Manufacturer version

        internal const int MESSAGE_LEVEL_WIDTH = 8; // Width to which the message level will be padded
        internal const int TEST_NAME_WIDTH = 35; // Width allowed for test names in screen display and log files
        internal const string APPLICATION_FOLDER_NAME = @"ASCOM\Sentinel"; // Application folder name underneath the local application data folder
        internal const string SETTINGS_FILENAME = "sentinel.settings"; // Settings file name
        internal const string LOG_FILENAME = "sentinel.log"; // Log file name
        internal const string WELCOME_MESSAGE = $"Welcome to {APPLICATION_NAME}!"; // Welcome message
        internal const string MANUFACTURER_NAME = "Peter Simpson"; // Manufacturer name 
        internal const int DEFAULT_ALPACA_PORT = 32324; // Default Alpaca port
        internal const int MAXIMUM_LOG_SIZE_CHARACTERS = 100000; // Maximum log file size in characters before truncation
        internal const int LOG_TRUNCATION_CHARACTERS = 5000; // The number of characters by which to truncate the screen log when required.
        internal const bool ALPACA_SEARCH_STRICT_CASING = false;
        internal const string USER_AGENT_PRODUCT_NAME = "ObservatoryManager";

        internal const string SAFETY_MONITOR_DEVICE_NAME = $"{APPLICATION_NAME} - Safety Monitor";
        internal const string OBSERVING_CONDITIONS_DEVICE_NAME = $"{APPLICATION_NAME}  - Observing Conditions";

        internal const string SAFETY_EVENT_ACTION_NAME = "GetSafetyState";
        internal const string SAFETY_EVENT_ACTION_NAME_LOWERCASE = "getsafetystate"; // Lowercase version of the action name for comparison when strict casing is disabled

        internal const string NOT_SET = "Not configured";

        internal static readonly Lock writeLogLock = new(); // Lock object to synchronize access to the log when resizing

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
    }
}
