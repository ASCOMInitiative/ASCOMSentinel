namespace ObsMan
{
    internal static class Globals
    {
        internal const int MESSAGE_LEVEL_WIDTH = 8; // Width to which the message level will be padded
        internal const int TEST_NAME_WIDTH = 35; // Width allowed for test names in screen display and log files
        internal const string APPLICATION_FOLDER_NAME = @"ASCOM\obsman"; // Application folder name underneath the local application data folder
        internal const string SETTINGS_FILENAME = "obsman.settings"; // Settings file name
        internal const string LOG_FILENAME = "obsman.log"; // Log file name
        internal const string WELCOME_MESSAGE = "Welcome to Observatory Manager!"; // Welcome message
        internal const string MANUFACTURER_NAME = "Peter Simpson"; // Manufacturer name 
        internal const string MANUFACTURER_VERSION = "0.0.1"; // Manufacturer version
        internal const int DEFAULT_ALPACA_PORT = 32324; // Default Alpaca port
        internal const int MAXIMUM_LOG_SIZE_CHARACTERS = 100000; // Maximum log file size in characters before truncation
        internal const int LOG_TRUNCATION_CHARACTERS = 5000; // The number of characters by which to truncate the screen log when required.
        internal const bool ALPACA_SEARCH_STRICT_CASING = false;
        internal const string USER_AGENT_PRODUCT_NAME = "ObservatoryManager";
    }
}
