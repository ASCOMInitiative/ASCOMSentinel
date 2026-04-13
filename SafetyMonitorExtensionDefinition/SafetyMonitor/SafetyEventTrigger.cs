namespace SafetyMonitorExtension
{
    /// <summary>
    /// The condition that triggered the safety event.
    /// </summary>
    /// <remarks>Always use the most specific description available.</remarks>
    public enum SafetyEventTrigger
    {
        /// <summary>
		/// Uninitialised value placeholder. This value should never be used in practice and is reserved to help identify uninitialised variables.
		/// </summary>
		ValueNotSet = 0,

        /// <summary>
        /// The monitored value has fallen below the safety threshold defined for this property.
        /// </summary>
        BelowThreshold = 1,

        /// <summary>
        /// The monitored value has reached the safety threshold defined for this property.
        /// </summary>
        AtThreshold = 2,

        /// <summary>
        /// The monitored value has exceeded the safety threshold defined for this property.
        /// </summary>
        AboveThreshold = 3,

        /// <summary>
        /// An alarm or other trigger condition has been activated.
        /// </summary>
        Active = 4,

        /// <summary>
        /// A required service or device is not running.
        /// </summary>
        Inactive = 5,

        /// <summary>
        /// A required device or service is unavailable.
        /// </summary>
        Offline = 6,

        /// <summary>
        /// The monitored condition is unsafe.
        /// </summary>
        Unsafe = 7,

        /// <summary>
        /// The property has been forced to a specific state or value and should be treated as unsafe.
        /// </summary>
        /// <remarks>Useful for testing.</remarks>
        ForcedState = 8,

        /// <summary>
        /// The device is in an error condition.
        /// </summary>
        ErrorCondition = 9,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved1 = 10,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved2 = 11,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved3 = 12,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved4 = 13,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved5 = 14,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved6 = 15,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved7 = 16,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved8 = 17,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved9 = 18,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved10 = 19,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined1 = 20,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined2 = 21,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined3 = 22,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined4 = 23,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined5 = 24,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined6 = 25,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined7 = 26,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined8 = 27,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined9 = 28,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined10 = 29
    }
}
