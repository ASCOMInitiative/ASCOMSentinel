using ASCOM.Tools;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Xml.Linq;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace Sentinel
{
    public class Settings : IDisposable
    {
        #region Constants and variables

        private static LogLevel LOGGING_LEVEL = LogLevel.Information;

        private const int SETTINGS_COMPATIBILTY_VERSION = 1; // Current settings file version number

        private bool disposedValue;
        private readonly int settingsFileVersion;

        private static readonly JsonSerializerOptions jsonSerialisationOptions; // JSON De-serialisation options

        #endregion

        #region Initialisers and Dispose

        /// <summary>
        /// Static initialiser so we only need to set JSON de-serialisation options once
        /// </summary>
        static Settings()
        {
            // Set JSON de-serialisation options
            jsonSerialisationOptions = new()
            {
                PropertyNameCaseInsensitive = true, // Ignore incorrect element name casing
                WriteIndented = true
            };
            jsonSerialisationOptions.Converters.Add(new JsonStringEnumConverter()); // For increased resilience, accept both string member names and integer member values as valid for enum elements.
        }

        public Settings()
        {
            LogMessage(LogLevel.Debug, "Settings () Initiator");
            Status = "Default settings in use.";
            //string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME);
            //SettingsFileName = Path.Combine(folderName, Globals.SETTINGS_FILENAME);
        }

        /// <summary>
        /// Create a Configuration management instance and load the current settings
        /// </summary>
        /// <param name="logger">Data logger instance.</param>
        public Settings(string configurationFile)
        {
            LogMessage(LogLevel.Debug, $"Settings(configurationFile) Initiator - {(string.IsNullOrEmpty(configurationFile) ? "Using default file location" : $"Using supplied file location: {configurationFile}")}");
            try
            {
                // Get the full settings file name including path
                if (string.IsNullOrEmpty(configurationFile)) // No override settings fie has been specified so use the application default settings file
                {
                    string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME);
                    SettingsFileName = Path.Combine(folderName, Globals.SETTINGS_FILENAME);
                }
                else // An override settings file has been supplied so use it instead of the default settings file
                {
                    SettingsFileName = configurationFile;
                }
                LogMessage(LogLevel.Information, $"Loading settings from file: {SettingsFileName}");

                // Load the values in the settings file if it exists
                if (File.Exists(SettingsFileName)) // Settings file exists
                {
                    // Read the file contents into a string
                    LogMessage(LogLevel.Debug, "File exists, about to read it...");
                    string serialisedSettingsString = File.ReadAllText(SettingsFileName);
                    //LogMessage(LogLevel.Debug, $"Serialised settings:\r\n{serialisedSettingsString}");

                    // Make a basic check to see if this file is a beta / pre-release version that doesn't have a version number. If so replace with a new version
                    LogMessage(LogLevel.Debug, $"Found compatibility version element...");
                    // Try to read in the settings version number from the settings file
                    try
                    {
                        // Get the settings version number by parsing the settings string
                        LogMessage(LogLevel.Debug, $"About to parse settings string");
                        using (JsonDocument appSettingsDocument = JsonDocument.Parse(serialisedSettingsString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }))
                        {
                            LogMessage(LogLevel.Debug, $"About to get settings version");
                            settingsFileVersion = appSettingsDocument.RootElement.GetProperty(nameof(SettingsCompatibilityVersion)).GetInt32();
                            LogMessage(LogLevel.Debug, $"Found settings version: {settingsFileVersion}");
                        }

                        // Handle different file versions
                        switch (settingsFileVersion)
                        {
                            // File version 1 - first production release
                            case 1:
                                try
                                {
                                    // De-serialise the settings string into a Settings object
                                    Settings? settings = JsonSerializer.Deserialize<Settings>(serialisedSettingsString, jsonSerialisationOptions);
                                    if (settings is null)
                                        settings = new Settings();

                                    // Test whether the retrieved settings match the requirements of this version of Observatory Manager
                                    if (settings.SettingsCompatibilityVersion == Settings.SETTINGS_COMPATIBILTY_VERSION) // Version numbers match so all is well
                                    {
                                        Status = $"Settings read OK.";
                                        LogMessage(LogLevel.Information, $"Settings read OK");

                                        // Load the retrieved settings into this instance
                                        CopyPropertiesFrom(settings);
                                    }
                                    else // Version numbers don't match so reset to defaults
                                    {
                                        int originalSettingsCompatibilityVersion = 0;
                                        try
                                        {
                                            originalSettingsCompatibilityVersion = settings.SettingsCompatibilityVersion;

                                            // Rename the current settings file to preserve it
                                            string badVersionSettingsFileName = $"{SettingsFileName}.badversion";
                                            File.Delete(badVersionSettingsFileName);
                                            File.Move(SettingsFileName, $"{badVersionSettingsFileName}");

                                            // Persist the default settings values
                                            ResetToDefaults();

                                            Status = $"The current settings version: {originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION}. Application settings have been reset to default values and the original settings file renamed to {badVersionSettingsFileName}.";
                                            LogMessage(LogLevel.Warning, $"The current settings version: {originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION}.");
                                            LogMessage(LogLevel.Warning, $"Application settings have been reset to default values and the original settings file renamed to {badVersionSettingsFileName}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage(LogLevel.Error, $"Error persisting new settings file: {ex.Message}\r\n{ex}");
                                            Status = $"The current settings version:{originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION} but the new settings could not be saved: {ex.Message}.";
                                        }
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    // There was an exception when parsing the settings file so report it and set default values
                                    LogMessage(LogLevel.Error, $"Error de-serialising settings file: {ex.Message}\r\n{ex}");
                                    Status = $"There was an error de-serialising the settings file and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to save new values.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                                }
                                catch (Exception ex)
                                {
                                    LogMessage(LogLevel.Error, ex.ToString());
                                    Status = $"Exception reading the settings file, default values are in effect.";
                                }
                                break;

                            // Handle unknown settings version numbers
                            default:

                                // Persist default settings values because the file version is unknown and the file may be corrupt
                                try
                                {
                                    // Rename the current settings file to preserve it
                                    string badVersionSettingsFileName = $"{SettingsFileName}.unknownversion";
                                    File.Delete(badVersionSettingsFileName);
                                    File.Move(SettingsFileName, $"{badVersionSettingsFileName}");

                                    // Persist the default settings values
                                    ResetToDefaults();

                                    Status = $"An unsupported settings version was found: {settingsFileVersion}. Settings have been reset to defaults and the original settings file has been renamed to {badVersionSettingsFileName}.";
                                    LogMessage(LogLevel.Warning, $"An unsupported settings version was found: {settingsFileVersion}.");
                                    LogMessage(LogLevel.Warning, $"Application settings have been reset to default values and the original settings file renamed to {badVersionSettingsFileName}.");
                                }
                                catch (Exception ex2)
                                {
                                    LogMessage(LogLevel.Error, $"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new settings: {ex2}");
                                    Status = $"$\"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new settings: {ex2.Message}.";
                                }
                                break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // There was an exception when parsing the settings file so report it and use default values
                        LogMessage(LogLevel.Error, $"Error getting settings file version from settings file: {ex.Message}\r\n{ex}");
                        Status = $"An error occurred when reading the settings file version and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to create a new settings file.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        LogMessage(LogLevel.Error, $"Exception parsing the settings file: {ex.Message}\r\n{ex}");
                        Status = $"Exception parsing the settings file: {ex.Message}";
                    }
                }
                else // Settings file does not exist
                {
                    LogMessage(LogLevel.Information, $"Settings file does not exist, initialising new file: {SettingsFileName}");
                    ResetToDefaults();
                    Status = $"First time use - configuration set to default values.";
                }
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"Load settings exception: {ex.Message}\r\n{ex}");
                Status = $"Unexpected exception reading the settings file, default values are in use.";
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LogMessage(LogLevel.Debug, "LoadSettings.Dispose()...");
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put clean-up code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public persisted properties

        // NOTE Values to be persisted must be defined as PROPERTIES rather than FIELDS.
        // If they are not properties they will NOT be included in the serialised JSON string.
        public Dictionary<PropertyName, SafetyMonitorState> SafetyMonitorSettings { get; set; } = new()
        {
            { PropertyName.SafetyMonitor0, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor1, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor2, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor3, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor4, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor5, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor6, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor7, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor8, SafetyMonitorState.Enabled },
            { PropertyName.SafetyMonitor9, SafetyMonitorState.Enabled },
        };
        public Dictionary<PropertyName, ObservingConditionsRule> ObservingCondtionsRules { get; set; } = new()
        {
            { PropertyName.CloudCover, new ObservingConditionsRule() },
            { PropertyName.DewPoint, new ObservingConditionsRule() },
            { PropertyName.Humidity, new ObservingConditionsRule() },
            { PropertyName.Pressure, new ObservingConditionsRule() },
            { PropertyName.RainRate, new ObservingConditionsRule() },
            { PropertyName.SkyBrightness, new ObservingConditionsRule() },
            { PropertyName.SkyQuality, new ObservingConditionsRule() },
            { PropertyName.SkyTemperature, new ObservingConditionsRule() },
            { PropertyName.StarFWHM, new ObservingConditionsRule() },
            { PropertyName.Temperature, new ObservingConditionsRule() },
            { PropertyName.WindDirection, new ObservingConditionsRule() },
            { PropertyName.WindGust, new ObservingConditionsRule() },
            { PropertyName.WindSpeed, new ObservingConditionsRule() },
        };
        public Dictionary<PropertyName, DiscoveredDevice> ConfiguredDevices { get; set; } = new()
        {
            { PropertyName.CloudCover, new DiscoveredDevice() },
            { PropertyName.DewPoint, new DiscoveredDevice() },
            { PropertyName.Humidity, new DiscoveredDevice() },
            { PropertyName.Pressure, new DiscoveredDevice() },
            { PropertyName.RainRate, new DiscoveredDevice() },
            { PropertyName.SkyBrightness, new DiscoveredDevice() },
            { PropertyName.SkyQuality, new DiscoveredDevice() },
            { PropertyName.SkyTemperature, new DiscoveredDevice() },
            { PropertyName.StarFWHM, new DiscoveredDevice() },
            { PropertyName.Temperature, new DiscoveredDevice() },
            { PropertyName.WindDirection, new DiscoveredDevice() },
            { PropertyName.WindGust, new DiscoveredDevice() },
            { PropertyName.WindSpeed, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor0, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor1, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor2, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor3, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor4, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor5, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor6, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor7, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor8, new DiscoveredDevice() },
            { PropertyName.SafetyMonitor9, new DiscoveredDevice() }
        };

        public Dictionary<PropertyName, GaugeConfiguration> GaugeSettings { get; set; } = new()
        {
            { PropertyName.CloudCover, new GaugeConfiguration() },
            { PropertyName.DewPoint, new GaugeConfiguration() },
            { PropertyName.Humidity, new GaugeConfiguration() },
            { PropertyName.Pressure, new GaugeConfiguration() },
            { PropertyName.RainRate, new GaugeConfiguration() },
            { PropertyName.SkyBrightness, new GaugeConfiguration() },
            { PropertyName.SkyQuality, new GaugeConfiguration() },
            { PropertyName.SkyTemperature, new GaugeConfiguration() },
            { PropertyName.StarFWHM, new GaugeConfiguration() },
            { PropertyName.Temperature, new GaugeConfiguration() },
            { PropertyName.WindDirection, new GaugeConfiguration() },
            { PropertyName.WindGust, new GaugeConfiguration() },
            { PropertyName.WindSpeed, new GaugeConfiguration() }
        };

        public bool LogSafetyWarnings { get; set; } = true;
        public bool ConfirmRestart { get; set; } = true;
        public bool ConfirmReset { get; set; } = true;
        public bool ConfirmConnectToDevices { get; set; } = true;
        public bool ConfirmReConnect { get; set; } = true;
        public bool ConfirmOnline { get; set; } = true;
        public double AlpacaDiscoveryDuration { get; set; } = 1.0; // Seconds
        public int AlpacaGetPropertyTimeout { get; set; } = 2; // Seconds to wait for a response when getting a value from an Alpaca device before timing out
        public bool AutoConnect { get; set; } = true;
        public TimeSpan PropertyCacheTime { get; set; } = TimeSpan.FromSeconds(1.0); // Seconds to wait before considering cached device property values as expired and retrieving new values from the devices
        public int AlpacaConnectTimeout { get; set; } = 10; // Seconds to wait for a response when connecting to an Alpaca device before timing out
        public bool IncludeAlpacaTrace { get; set; } = false;
        public int SettingsCompatibilityVersion { get; set; } = SETTINGS_COMPATIBILTY_VERSION;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string Location { get; set; } = "My Observatory";
        public string UniqueIdSafetyMonitor { get; set; } = Guid.NewGuid().ToString();
        public string UniqueIdSwitch { get; set; } = Guid.NewGuid().ToString();
        public string UniqueIdObservingConditions { get; set; } = Guid.NewGuid().ToString();
        public ushort ServerPort { get; set; } = (ushort)Globals.DEFAULT_ALPACA_PORT;
        public bool BindToAllNetworkAddresses { get; set; } = true;
        public bool AllowDiscovery { get; set; } = true;
        public bool DiscoveryResponseOnlyOnLocalHost { get; set; } = true;
        public bool EnableRemoteClients { get; set; } = true;
        public bool PreventRemoteDisconnects { get; set; } = true;
        public bool RunInStrictAlpacaMode { get; set; } = true;
        public bool RequireAdministratorLogin { get; set; } = false;
            public bool UseAuth { get; set; } = false;
            public string UserName { get; set; } = "User";
            public string Password { get; set; } = string.Empty;

        #endregion

        #region Public methods

        public void ResetToDefaults()
        {
            try
            {
                Settings defaults = new Settings();

                // Create serialised settings string with all values at defaults
                string serialisedSettings = JsonSerializer.Serialize<Settings>(defaults, jsonSerialisationOptions);

                // Create the settings folder if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));
                File.WriteAllText(SettingsFileName, serialisedSettings);

                // Reset all in-memory properties to their default values
                CopyPropertiesFrom(defaults);
                RaiseChangeEvent();
                Status = $"Settings reset at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"ResetToDefaults - Exception during Reset: {ex.Message}\r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Returns the stored unique ID for the given device type and number, creating and persisting one if it does not yet exist.
        /// </summary>
        internal string GetDeviceUniqueId(string deviceType, int deviceId)
        {
            return (deviceType, deviceId) switch
            {
                ("SafetyMonitor", 0) => UniqueIdSafetyMonitor,
                ("ObservingConditions", 0) => UniqueIdObservingConditions,
                ("Switch", 0) => UniqueIdSwitch,
                _ => Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Persist current settings
        /// </summary>
        public void Save()
        {
            LogMessage(LogLevel.Debug, "Saving settings to settings file");
            PersistSettings();
            Status = $"Settings saved at {DateTime.Now:HH:mm:ss}.";

            RaiseChangeEvent();
        }

        /// <summary>
        /// Raises the state changed event
        /// </summary>
        public void RaiseChangeEvent()
        {
            // Raise configuration has changed event
            if (ConfigurationChanged is not null)
            {
                try
                {
                    EventArgs args = new();
                    LogMessage(LogLevel.Debug, "Save settings - About to call configuration changed event handler");
                    ConfigurationChanged(this, args);
                    LogMessage(LogLevel.Debug, "Save settings - Returned from configuration changed event handler");
                }
                catch (Exception ex)
                {
                    LogMessage(LogLevel.Debug, $"RaiseChangeEvent - Exception during event handling: {ex.Message}\r\n{ex}");
                }
            }
        }

        #endregion

        #region Internal properties

        internal string SettingsFileName { get; private set; } = "";

        /// <summary>
        /// Text message describing any issues found when validating the settings
        /// </summary>
        internal string Status { get; private set; }

        #endregion

        #region Event handlers

        internal delegate void MessageEventHandler(object sender, MessageEventArgs e);

        internal event EventHandler? ConfigurationChanged;

        #endregion

        #region Support code

        public void LogMessage(LogLevel logLevel, string message)
        {
            try
            {
                // Check if the message should be logged based on the current log level setting
                if (logLevel >= LOGGING_LEVEL) // Message level is within or above the current log level
                {
                    lock (Globals.writeLogLock)
                    {
                        string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-13} {message}";

                        // Write the message to the console and color appropriately
                        Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                        var originalColour = Console.ForegroundColor;

                        // Select an appropriate colour for the log level
                        switch (logLevel)
                        {
                            case LogLevel.Debug:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case LogLevel.Information:
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                break;
                            case LogLevel.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case LogLevel.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                        }

                        Console.Write($"{logLevel,-13} ");
                        Console.ForegroundColor = originalColour;
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Settings.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        private void CopyPropertiesFrom(Settings source)
        {
            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                // Skip internal-only properties that must not be overwritten from a deserialized instance
                if (property.Name == nameof(SettingsFileName) || property.Name == nameof(Status))
                    continue;

                LogMessage(LogLevel.Debug, $"CopyPropertiesFrom - {property.Name} = {property.GetValue(source) ?? "null"}");
                try
                {
                    property.SetValue(this, property.GetValue(source));
                }
                catch
                {
                    // No action here because the property will take its default value when read.
                }
            }
        }

        private void PersistSettings()
        {
            try
            {
                // Set the version number of this settings file
                SettingsCompatibilityVersion = Settings.SETTINGS_COMPATIBILTY_VERSION;

                LogMessage(LogLevel.Debug, $"PersistSettings - Settings file: {SettingsFileName}");

                // Create serialised settings string containing current settings values
                string serialisedSettingsString = JsonSerializer.Serialize<Settings>(this, jsonSerialisationOptions);
                // LogMessage(LogLevel.Debug, $"PersistSettings - Serialised settings:\r\n{serialisedSettingsString}");

                // Create the settings folder if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));
                LogMessage(LogLevel.Debug, $"PersistSettings - Created directory. Writing to {SettingsFileName}");

                // Persist the settings to file
                File.WriteAllText(SettingsFileName, serialisedSettingsString);
            }
            catch (Exception ex)
            {
                LogMessage(LogLevel.Error, $"PersistSettings exception: {ex.Message}\r\n{ex}");
            }
        }

        #endregion

    }
}