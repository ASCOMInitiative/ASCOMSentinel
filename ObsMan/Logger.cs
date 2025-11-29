using ASCOM.Common.Interfaces;
using ASCOM.Tools;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using static ObsMan.Globals;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace ObsMan
{
    /// <summary>
    /// Initializes a new instance of the Logger class using the specified state service.
    /// </summary>
    /// <param name="state">The StateService instance that provides access to application state information for logging purposes. Cannot be
    /// null.</param>
    public class Logger : TraceLogger, ITraceLogger, IDisposable
    {
        State state;
        Settings settings;
        #region Initialisers

        static Logger()
        {
            // Ensure that text output always used UTF8 regardless of the setting of the parent application.
            // Without this change, piping output to another process can result in translation of single Unicode characters into multiple ASCII characters.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

        }

        public Logger(State state, Settings settings) : base("", "", LOG_FILENAME, true)
        {
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }


        #endregion

        #region Event handlers

        /// <summary>
        ///  Event fired when the message log changes.
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageLogChanged;

        #endregion

        #region Public methods

        /// <summary>
        /// Log a message on the screen, console and log file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        public void LogMessage(string id, LogLevel logLevel, string message, bool logToScreen = true)
        {
            try
            {
                string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel.ToString().PadRight(11)} {message}";
                // Write the message to the console
                Console.WriteLine(formattedMessage);

                // Write the message to the log file
                base.LogMessage(id, message);

                // Raise the MessaegLogChanged event to Write the message to the screen if required
                if (logToScreen) // Log to screen is enabled
                {
                    // Check if the message should be show
                    if (logLevel <= settings.LogLevel) // Message level is within or above the current log level
                    {
                        // Raise the MessaegLogChanged event to Write the message to the screen
                        state.ApplicationLog = state.ApplicationLog + $"\r\n{formattedMessage}";

                        OnMessageLogChanged(formattedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        public new void LogMessage(string method, string message)
        {
            lock (this)
            {
                // Write the message to the console
                Console.WriteLine($"{method}{(string.IsNullOrEmpty(method) ? "" : " ")}{message}");

                // Write the message to the log file
                base.LogMessage(method, message);


                OnMessageLogChanged($"{method} {message}");
            }
        }

        #endregion

        #region Support code

        private void OnMessageLogChanged(string message)
        {
            MessageEventArgs eventArgs = new()
            {
                Message = $"{DateTime.Now:HH:mm:ss.fff} {message}"
            };

            MessageLogChanged?.Invoke(this, eventArgs);
        }

        #endregion

    }
}
