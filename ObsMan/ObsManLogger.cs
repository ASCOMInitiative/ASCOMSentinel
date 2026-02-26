
using ASCOM.Common.Interfaces;
using ASCOM.Tools;
using ILogger = ASCOM.Common.Interfaces.ILogger;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace ObsMan
{
    public class ObsManLogger : TraceLogger, ITraceLogger, ILogger
    {
        State state;
        Settings settings;

        public ObsManLogger(State state, Settings settings) : base("ObsMan", true)
        { 
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }

        public ObsManLogger(string logFileName, State state, Settings settings) : base("ObsMan", true)
        {
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }

        #region Event handlers

        /// <summary>
        ///  Event fired when the message log changes.
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageLogChanged;

        #endregion

        void ILogger.Log(LogLevel level, string message)
        {
            //base.LogMessage(level.ToString(), message);
            //Console.WriteLine($"{level}: {message}");
            LogMessage(string.Empty, level, message);
        }

        /// <summary>
        /// Log a message on the screen, console and log file of the specified type: debug, information etc.
        /// </summary>
        /// <param name="method">Current method name</param>
        /// <param name="message">Message to log</param>
        /// <param name="logLevel">Importance - debug, information etc.</param>
        public void LogMessage(string method, LogLevel logLevel, string message, bool logToScreen = true)
        {
            try
            {
                // Check if the message should be logged based on the current log level setting
                if (logLevel >= settings.LogLevel) // Message level is within or above the current log level
                {
                    // Lock this method to prevent multiple threads writing to the log at the same time
                    lock (this)
                    {
                        string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-13} {message}";

                        // Write the message to the console and color appropriately
                        Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                        var originalColour = Console.ForegroundColor;

                        // Select an appropriate colour for the log level
                        switch (logLevel)
                        {
                            case LogLevel.Verbose:
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                break;
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

                        // Write the message to the log file
                        base.LogMessage(method, message);

                        // Raise the MessaegLogChanged event to Write the message to the screen if required
                        if (logToScreen) // Log to screen is enabled
                        {
                            // Update the screen log, truncating it if required
                            try
                            {
                                // Update the screen log
                                state.ApplicationLog.Append($"\r\n{formattedMessage}");
                            }
                            catch (ArgumentOutOfRangeException) // The new length exceeded the specified maximum so truncate the log
                            {
                                // Truncate the log
                                state.ApplicationLog.Remove(0, Globals.LOG_TRUNCATION_CHARACTERS);
                                state.ApplicationLog.Insert(0, $"\r\n**** Log truncated at {DateTime.Now:HH:mm:ss.fff} ****\r\n");

                                // Update the screen log
                                state.ApplicationLog.Append($"\r\n{formattedMessage}");
                            }

                            // Raise the MessaegLogChanged event to let listeners know that the log has been updated
                            OnMessageLogChanged(formattedMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger.LogMessage Exception: {ex.Message}\r\n{ex}");
            }
        }

        /// <summary>
        /// Log an information message on the screen, console and log file
        /// </summary>
        /// <param name="method">Current method name</param>
        /// <param name="message">Message to log</param>
        public new void LogMessage(string method, string message)
        {
            LogMessage(method, LogLevel.Information, message);
        }

        /// <summary>
        /// Log a debug message on the screen, console and log file
        /// </summary>
        /// <param name="method">Current method name</param>
        /// <param name="message">Message to log</param>
        public void LogDebug(string method, string message)
        {
            LogMessage(method, LogLevel.Debug, message);
        }

        /// <summary>
        /// Log an error message on the screen, console and log file
        /// </summary>
        /// <param name="method">Current method name</param>
        /// <param name="message">Message to log</param>
        public void LogError(string method, string message)
        {
            LogMessage(method, LogLevel.Error, message);
        }

        #region Support code

        /// <summary>
        /// Raises the MessageLogChanged event to notify subscribers that a new message has been added to the message log.
        /// </summary>
        /// <param name="message">The message text to include in the event notification. Cannot be null.</param>
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