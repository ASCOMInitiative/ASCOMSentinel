using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ASCOM.Common;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace ObsMan
{
    public class Settings : IDisposable
    {
        #region Constants and variables

        private const int SETTINGS_COMPATIBILTY_VERSION = 1; // Current settings file version number

        private Logger? TL;
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
            Status = "Default settings in use.";
        }

        /// <summary>
        /// Create a Configuration management instance and load the current settings
        /// </summary>
        /// <param name="logger">Data logger instance.</param>
        public Settings(Logger logger) : this(logger, "")
        {

        }

        /// <summary>
        /// Create a Configuration management instance and load the current settings
        /// </summary>
        /// <param name="logger">Data logger instance.</param>
        public Settings(Logger logger, string configurationFile)
        {
            TL = logger;

            try
            {
                // Get the full settings file name including path
                if (string.IsNullOrEmpty(configurationFile)) // No override settings fie has been specified so use the application default settings file
                {
                    string folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME);
                    SettingsFileName = Path.Combine(folderName, Globals.SETTINGS_FILENAME);
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Settings folder: {folderName}, Settings file: {SettingsFileName}");
                }
                else // An override settings file has been supplied so use it instead of the default settings file
                {
                    SettingsFileName = configurationFile;
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Settings file: {SettingsFileName}");
                }

                // Load the values in the settings file if it exists
                if (File.Exists(SettingsFileName)) // Settings file exists
                {
                    // Read the file contents into a string
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, "File exists, about to read it...");
                    string serialisedSettingsString = File.ReadAllText(SettingsFileName);
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Serialised settings:\r\n{serialisedSettingsString}");

                    // Make a basic check to see if this file is a beta / pre-release version that doesn't have a version number. If so replace with a new version
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Found compatibility version element...");
                    // Try to read in the settings version number from the settings file
                    try
                    {
                        // Get the settings version number by parsing the settings string
                        TL?.LogMessage("LoadSettings", LogLevel.Debug, $"About to parse settings string");
                        using (JsonDocument appSettingsDocument = JsonDocument.Parse(serialisedSettingsString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }))
                        {
                            TL?.LogMessage("LoadSettings", LogLevel.Debug, $"About to get settings version");
                            settingsFileVersion = appSettingsDocument.RootElement.GetProperty("SettingsCompatibilityVersion").GetInt32();
                            TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Found settings version: {settingsFileVersion}");
                        }

                        // Handle different file versions
                        switch (settingsFileVersion)
                        {
                            // File version 1 - first production release
                            case 1:
                                try
                                {
                                    // De-serialise the settings string into a Settings object
                                    Settings settings = JsonSerializer.Deserialize<Settings>(serialisedSettingsString, jsonSerialisationOptions) ?? new Settings();

                                    // Test whether the retrieved settings match the requirements of this version of ConformU
                                    if (settings.SettingsCompatibilityVersion == Settings.SETTINGS_COMPATIBILTY_VERSION) // Version numbers match so all is well
                                    {
                                        Status = $"Settings read OK.";

                                        // Load the retrieved settings into this instance
                                        PropertyInfo[] properties = settings.GetType().GetProperties(); // Get a list of public properties in this class
                                        foreach (PropertyInfo property in properties)
                                        {
                                            string name = property.Name;
                                            object value = property.GetValue(settings) ?? "Null value";

                                            logger.LogDebug($"Settings is loading property: {name} = {value}");

                                            // Try to set the property but ignore bad values, which will take the property's default value instead
                                            try
                                            {
                                                // Get the value of the property from the new settings class and set it into this class's property
                                                property.SetValue(this, property.GetValue(settings));
                                            }
                                            catch
                                            {
                                                // No action here because the property will take its default value when read.                                            }
                                            }
                                        }
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
                                        }
                                        catch (Exception ex2)
                                        {
                                            TL?.LogMessage("LoadSettings", LogLevel.Error, $"Error persisting new Conform settings file: {ex2}");
                                            Status = $"The current settings version:{originalSettingsCompatibilityVersion} does not match the required version: {Settings.SETTINGS_COMPATIBILTY_VERSION} but the new settings could not be saved: {ex2.Message}.";
                                        }
                                    }
                                }
                                catch (JsonException ex1)
                                {
                                    // There was an exception when parsing the settings file so report it and set default values
                                    TL?.LogMessage("LoadSettings", LogLevel.Error, $"Error de-serialising Conform settings file: {ex1}");
                                    Status = $"There was an error de-serialising the settings file and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to save new values.\r\n\r\nJSON parser error message:\r\n{ex1.Message}";
                                }
                                catch (Exception ex1)
                                {
                                    TL?.LogMessage("LoadSettings", LogLevel.Error, ex1.ToString());
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
                                }
                                catch (Exception ex2)
                                {
                                    TL?.LogMessage("LoadSettings", LogLevel.Error, $"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new Conform settings: {ex2}");
                                    Status = $"$\"An unsupported settings version was found: {settingsFileVersion} but an error occurred when saving new Conform settings: {ex2.Message}.";
                                }
                                break;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // There was an exception when parsing the settings file so report it and use default values
                        TL?.LogMessage("LoadSettings", LogLevel.Error, $"Error getting settings file version from settings file: {ex}");
                        Status = $"An error occurred when reading the settings file version and application default settings are in effect.\r\n\r\nPlease correct the error in the file or use the \"Reset to Defaults\" button on the Settings page to create a new settings file.\r\n\r\nJSON parser error message:\r\n{ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        TL?.LogMessage("LoadSettings", LogLevel.Error, $"Exception parsing the settings file: {ex}");
                        Status = $"Exception parsing the settings file: {ex.Message}";
                    }
                }
                else // Settings file does not exist
                {
                    TL?.LogMessage("LoadSettings", LogLevel.Debug, $"Configuration file does not exist, initialising new file: {SettingsFileName}");
                    ResetToDefaults();
                    Status = $"First time use - configuration set to default values.";
                }
            }
            catch (Exception ex)
            {
                TL?.LogMessage("LoadSettings", LogLevel.Error, ex.ToString());
                Status = $"Unexpected exception reading the settings file, default values are in use.";
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Console.WriteLine("LoadSettings.Dispose()...");
                    TL = null;
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

        public int SettingsCompatibilityVersion { get; set; } = SETTINGS_COMPATIBILTY_VERSION;

        public bool TraceOn { get; set; } = false;

        #endregion

        #region Public methods

        public void ResetToDefaults()
        {
            try
            {
                // Create serialised settings string with all values at defaults
                string serialisedSettings = JsonSerializer.Serialize<Settings>(new Settings(), jsonSerialisationOptions);

                // Create the settings folder if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));
                File.WriteAllText(SettingsFileName, serialisedSettings);

                Status = $"Settings reset at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                TL?.LogMessage("Reset", LogLevel.Error, $"Exception during Reset: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Persist current settings
        /// </summary>
        public void Save()
        {
            TL?.LogMessage("Save", LogLevel.Debug, "Saving settings to settings file");
            PersistSettings();
            Status = $"Settings saved at {DateTime.Now:HH:mm:ss}.";

            // Raise configuration has changed event
            if (ConfigurationChanged is not null)
            {
                EventArgs args = new();
                TL?.LogMessage("Save", LogLevel.Debug, "About to call configuration changed event handler");
                ConfigurationChanged(this, args);
                TL?.LogMessage("Save", LogLevel.Debug, "Returned from configuration changed event handler");
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

        private void PersistSettings()
        {
            try
            {
                // Set the version number of this settings file
                SettingsCompatibilityVersion = Settings.SETTINGS_COMPATIBILTY_VERSION;

                TL?.LogMessage("PersistSettings", LogLevel.Debug, $"Settings file: {SettingsFileName}");

                // Create serialised settings string containing current settings values
                string serialisedSettingsString = JsonSerializer.Serialize<Settings>(this, jsonSerialisationOptions);
                TL?.LogMessage("PersistSettings", LogLevel.Debug, $"Serialised settings:\r\n{serialisedSettingsString}");

                // Create the settings folder if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFileName) ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Globals.APPLICATION_FOLDER_NAME));

                // Persist the settings to file
                File.WriteAllText(SettingsFileName, serialisedSettingsString);
            }
            catch (Exception ex)
            {
                TL?.LogMessage("PersistSettings", LogLevel.Debug, ex.ToString());
            }

        }

        #endregion

    }
}