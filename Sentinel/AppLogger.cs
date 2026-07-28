using ASCOM.Common.Interfaces;
using ASCOM.Tools;
using ILogger = ASCOM.Common.Interfaces.ILogger;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace Sentinel
{
    public class AppLogger : TraceLogger, IAppLogger
    {
        private readonly State state;
        private readonly Settings settings;

        #region Constructors

        public AppLogger(string logName, State state, Settings settings) : base(logName, true)
        {
            this.state = state;
            this.settings = settings;
            SetMinimumLoggingLevel(settings.LogLevel);
        }

        #endregion

        #region Public events

        public event EventHandler<MessageEventArgs>? MessageLogChanged;

        #endregion

        #region Public overwritten TraceLogger methods to ensure that logging is handled by this logger

        public new void Log(LogLevel level, string message)
        {
            LogMessage(string.Empty, level, message);
        }
        public new void LogMessage(string method, string message) => LogMessage(method, LogLevel.Information, message);
        public new void BlankLine() => LogBlankLine();

        #endregion

        #region  Public methods unique to this logger

        public void LogMessage(string method, LogLevel logLevel, string message, bool logToScreen = true)
        {
            try
            {
                message ??= string.Empty;

                if (message.Contains(Globals.DISCOVERY_PACKET_MESSAGE, StringComparison.OrdinalIgnoreCase) && !settings.LogDiscoveryMessages)
                    return;

                if (logLevel >= settings.LogLevel)
                {
                    lock (Globals.writeLogLock)
                    {
                        string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-13} {message}";
                        Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
                        ConsoleColor originalColour = Console.ForegroundColor;

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

                        base.LogMessage(method, message);

                        if (logToScreen)
                        {
                            try
                            {
                                state.ApplicationLog.Append($"\r\n{formattedMessage}");
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                int originalLength = state.ApplicationLog.Length;
                                state.ApplicationLog.Remove(0, Globals.LOG_TRUNCATION_CHARACTERS);
                                int newLength = state.ApplicationLog.Length;
                                state.ApplicationLog.Insert(0, $"\r\n**** Log truncated at {DateTime.Now:HH:mm:ss.fff} ****\r\n");
                                state.ApplicationLog.Append($"{formattedMessage}");
                            }

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
        public void LogDebug(string method, string message) => LogMessage(method, LogLevel.Debug, message);
        public void LogWarning(string method, string message) => LogMessage(method, LogLevel.Warning, message);
        public void LogError(string method, string message) => LogMessage(method, LogLevel.Error, message);
        public void LogVerbose(string method, string message) => LogMessage(method, LogLevel.Verbose, message);
        public void LogMessageConsole(string method, string message) => LogMessage(method, LogLevel.Information, message, logToScreen: false);
        public void LogDebugConsole(string method, string message) => LogMessage(method, LogLevel.Debug, message, logToScreen: false);
        public void LogWarningConsole(string method, string message) => LogMessage(method, LogLevel.Warning, message, logToScreen: false);
        public void LogErrorConsole(string method, string message) => LogMessage(method, LogLevel.Error, message, logToScreen: false);
        public void LogBlankLine() => LogMessage(string.Empty, string.Empty);
        public void LogWarning(string message) => LogWarning(string.Empty, message);
        public void LogError(string message) => LogError(string.Empty, message);
        public void ClearScreen()
        {
            lock (Globals.writeLogLock)
            {
                state.ApplicationLog.Clear();
            }
        }

        /// <summary>
        /// Append the specified number of new lines to the output
        /// </summary>
        /// <param name="count">The number of new lines to add</param>
        public void Newlines(int count)
        {
            lock (Globals.writeLogLock)
            {
                for (int i = 0; i < count; i++)
                {
                    // Try to append the line
                    try
                    {
                        state.ApplicationLog.AppendLine();
                        // Appended successfully
                    }
                    catch (ArgumentOutOfRangeException) // Exceeded the screen log length so truncate it
                    {
                        // Truncate the screen log
                        state.ApplicationLog.Remove(0, Globals.LOG_TRUNCATION_CHARACTERS);

                        // Write the new line to the truncated screen log
                        state.ApplicationLog.AppendLine();

                        // Add a record to the written log
                        base.LogMessage(string.Empty, string.Empty);
                    }
                }

                // Notify listeners that the screen log has changed.
                OnMessageLogChanged("\r\n");
            }
        }

        #endregion

        #region Private members

        private void OnMessageLogChanged(string message)
        {
            MessageEventArgs eventArgs = new()
            {
                Message = $"{DateTime.Now:HH:mm:ss.fff} {message}"
            };

            MessageLogChanged?.Invoke(this, eventArgs);
        }

        #endregion


        //State state;
        //Settings settings;

        //public AppLogger(State state, Settings settings) : base("Sentinel", true)
        //{
        //    this.state = state;
        //    this.settings = settings;
        //    SetMinimumLoggingLevel(settings.LogLevel);
        //}

        //public AppLogger(string logFileName, State state, Settings settings) : base("Sentinel", true)
        //{
        //    this.state = state;
        //    this.settings = settings;
        //    SetMinimumLoggingLevel(settings.LogLevel);
        //}

        //#region Event handlers

        ///// <summary>
        /////  Event fired when the message log changes.
        ///// </summary>
        //public event EventHandler<MessageEventArgs>? MessageLogChanged;

        //#endregion

        //void ILogger.Log(LogLevel level, string message)
        //{
        //    //Console.WriteLine($"{level}: {message}");
        //    LogMessage(string.Empty, level, message);
        //}

        ///// <summary>
        ///// Log a message on the screen, console and log file of the specified type: debug, information etc.
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        ///// <param name="logLevel">Importance - debug, information etc.</param>
        //public void LogMessage(string method, LogLevel logLevel, string message, bool logToScreen = true)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(message))
        //        {
        //            message = string.Empty;
        //        }

        //        if (message.Contains(Globals.DISCOVERY_PACKET_MESSAGE,StringComparison.OrdinalIgnoreCase) && !settings.LogDiscoveryMessages)
        //            return;

        //        // Check if the message should be logged based on the current log level setting
        //        if (logLevel >= settings.LogLevel) // Message level is within or above the current log level
        //        {
        //            lock (Globals.writeLogLock)
        //            {
        //                string formattedMessage = $"{DateTime.Now:HH:mm:ss.fff} {logLevel,-13} {message}";

        //                // Write the message to the console and color appropriately
        //                Console.Write($"{DateTime.Now:HH:mm:ss.fff} ");
        //                var originalColour = Console.ForegroundColor;

        //                // Select an appropriate colour for the log level
        //                switch (logLevel)
        //                {
        //                    case LogLevel.Verbose:
        //                        Console.ForegroundColor = ConsoleColor.DarkCyan;
        //                        break;
        //                    case LogLevel.Debug:
        //                        Console.ForegroundColor = ConsoleColor.Blue;
        //                        break;
        //                    case LogLevel.Information:
        //                        Console.ForegroundColor = ConsoleColor.DarkGreen;
        //                        break;
        //                    case LogLevel.Warning:
        //                        Console.ForegroundColor = ConsoleColor.Yellow;
        //                        break;
        //                    case LogLevel.Error:
        //                        Console.ForegroundColor = ConsoleColor.Red;
        //                        break;
        //                    default:
        //                        Console.ForegroundColor = ConsoleColor.White;
        //                        break;
        //                }

        //                Console.Write($"{logLevel,-13} ");
        //                Console.ForegroundColor = originalColour;
        //                Console.WriteLine(message);

        //                // Write the message to the log file
        //                base.LogMessage(method, message);

        //                // Raise the MessaegLogChanged event to Write the message to the screen if required
        //                if (logToScreen) // Log to screen is enabled
        //                {
        //                    // Update the screen log, truncating it if required
        //                    try
        //                    {
        //                        // Update the screen log
        //                        state.ApplicationLog.Append($"\r\n{formattedMessage}");
        //                    }
        //                    catch (ArgumentOutOfRangeException ex) // The new length exceeded the specified maximum so truncate the log
        //                    {
        //                        // Truncate the log
        //                        int originalLength= state.ApplicationLog.Length;
        //                        state.ApplicationLog.Remove(0, Globals.LOG_TRUNCATION_CHARACTERS);
        //                        int newLength = state.ApplicationLog.Length;
        //                        state.ApplicationLog.Insert(0, $"\r\n**** {ex.Message} Log truncated at {DateTime.Now:HH:mm:ss.fff} original length: {originalLength}, new length: {newLength} ****\r\n");

        //                        // Update the screen log
        //                        state.ApplicationLog.Append($"\r\n{formattedMessage}");
        //                    }
        //                    // Raise the MessaegLogChanged event to let listeners know that the log has been updated
        //                    OnMessageLogChanged(formattedMessage);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Logger.LogMessage Exception: {ex.Message}\r\n{ex}");
        //    }
        //}

        ///// <summary>
        ///// Log an information message on the screen, console and log file
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        //public new void LogMessage(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Information, message);
        //}

        ///// <summary>
        ///// Log a debug message on the screen, console and log file
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        //public void LogDebug(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Debug, message);
        //}

        ///// <summary>
        ///// Log a warning message on the screen, console and log file
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        //public void LogWarning(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Warning, message);
        //}

        ///// <summary>
        ///// Log an error message on the screen, console and log file
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        //public void LogError(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Error, message);
        //}

        ///// <summary>
        ///// Log a debug message on the screen, console and log file
        ///// </summary>
        ///// <param name="method">Current method name</param>
        ///// <param name="message">Message to log</param>
        //public void LogVerbose(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Verbose, message);
        //}
        //public void LogMessageConsole(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Information, message, logToScreen: false);
        //}

        //public void LogDebugConsole(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Debug, message, logToScreen: false);
        //}

        //public void LogWarningConsole(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Warning, message, logToScreen: false);
        //}

        //public void LogErrorConsole(string method, string message)
        //{
        //    LogMessage(method, LogLevel.Error, message, logToScreen: false);
        //}

        //public void LogBlankLine()
        //{
        //    LogMessage("", "");
        //}


        //#region Support code

        ///// <summary>
        ///// Raises the MessageLogChanged event to notify subscribers that a new message has been added to the message log.
        ///// </summary>
        ///// <param name="message">The message text to include in the event notification. Cannot be null.</param>
        //private void OnMessageLogChanged(string message)
        //{
        //    MessageEventArgs eventArgs = new()
        //    {
        //        Message = $"{DateTime.Now:HH:mm:ss.fff} {message}"
        //    };

        //    MessageLogChanged?.Invoke(this, eventArgs);
        //}

        //#endregion

    }
}