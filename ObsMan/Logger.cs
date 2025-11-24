using ASCOM.Common.Interfaces;
using ASCOM.Tools;
using static ObsMan.Globals;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace ObsMan
{
    public class Logger : TraceLogger, ITraceLogger, IDisposable
    {
        private bool debug;

        #region Initialisers

        static Logger()
        {
            // Ensure that text output always used UTF8 regardless of the setting of the parent application.
            // Without this change, piping output to another process can result in translation of single Unicode characters into multiple ASCII characters.
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public Logger() : base("", "", LOG_FILENAME, true)
        {
            this.debug = true;
        }
        public Logger(string logFileName, string logFilePath, string loggerName, bool enabled) : base(logFileName, logFilePath, loggerName, enabled)
        {
            base.IdentifierWidth = TEST_NAME_WIDTH;
        }

        #endregion

        #region Event handlers

        /// <summary>
        ///  Event fired when the message log changes.
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageLogChanged;

        /// <summary>
        /// Event fired when the status message changes.
        /// </summary>
        public event EventHandler<MessageEventArgs>? StatusChanged;

        #endregion

        #region Public methods

        /// <summary>
        /// Flag indicating whether debug messages should be included in the log.
        /// </summary>
        public bool Debug
        {
            get
            {
                return debug;
            }
            set
            {
                debug = value;
                if (value)
                {
                    base.SetMinimumLoggingLevel(ASCOM.Common.Interfaces.LogLevel.Debug);
                }
                else
                {
                    base.SetMinimumLoggingLevel(ASCOM.Common.Interfaces.LogLevel.Information);
                }
            }
        }

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
                // Write the message to the console
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {logLevel.ToString().PadRight(11)} {message}");

                // Write the message to the log file
                base.LogMessage(id, message);

                // Raise the MessaegLogChanged event to Write the message to the screen if required
                if (logToScreen)
                    OnMessageLogChanged(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        public new void LogMessage(string method, string message)
        {
            // Write the message to the console
            Console.WriteLine($"{method}{(string.IsNullOrEmpty(method) ? "" : " ")}{message}");

            // Write the message to the log file
            base.LogMessage(method, message);

            // Raise the MessaegLogChanged event to Write the message to the screen
            OnMessageLogChanged($"{method} {message}");

        }

        /// <summary>
        /// Raises an event notifying that the status message has changed
        /// </summary>
        /// <param name="status">new status message.</param>
        /// <remarks>
        /// This is part of ConformLogger for convenience because the logger is used everywhere in the application and already supports the LogMessageChanged event.
        /// </remarks>
        public void SetStatusMessage(string status)
        {
            MessageEventArgs eventArgs = new()
            {
                Message = status
            };

            EventHandler<MessageEventArgs>? messageEventHandler = StatusChanged;

            if (messageEventHandler is not null)
            {
                messageEventHandler(this, eventArgs);
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
