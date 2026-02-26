namespace ObsMan
{
    public static class ExtensionMethods
    {
        public static LogLevel ToMSLogLevel(this ASCOM.Common.Interfaces.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case ASCOM.Common.Interfaces.LogLevel.Verbose:
                case ASCOM.Common.Interfaces.LogLevel.Debug:
                    return LogLevel.Debug;

                case ASCOM.Common.Interfaces.LogLevel.Information:
                    return LogLevel.Information;

                case ASCOM.Common.Interfaces.LogLevel.Warning:
                    return LogLevel.Warning;

                case ASCOM.Common.Interfaces.LogLevel.Error:
                    return LogLevel.Error;

                default:
                    throw new ArgumentException($"Unknown logging level: {logLevel}");
            }
        }
    }
}
