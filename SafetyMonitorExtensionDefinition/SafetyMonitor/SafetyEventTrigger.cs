namespace SafetyMonitor
{
	/// <summary>
	/// The condition that triggered the safety event.
	/// </summary>
	/// <remarks>Always use the most specific description available.</remarks>
	public enum SafetyEventTrigger
	{
		/// <summary>
		/// The monitored value has fallen below the safety threshold defined for this property.
		/// </summary>
		BelowThreshold = 0,

		/// <summary>
		/// The monitored value has reached the safety threshold defined for this property.
		/// </summary>
		AtThreshold = 1,

		/// <summary>
		/// The monitored value has exceeded the safety threshold defined for this property.
		/// </summary>
		AboveThreshold = 2,

		/// <summary>
		/// An alarm or other trigger condition has been activated.
		/// </summary>
		Active = 3,

		/// <summary>
		/// A required service or device is not running.
		/// </summary>
		Inactive = 4,

		/// <summary>
		/// A required device or service is unavailable.
		/// </summary>
		Offline = 5,

		/// <summary>
		/// The monitored condition is unsafe.
		/// </summary>
		Unsafe = 6,

		/// <summary>
		/// The property has been forced to a specific state or value and should be treated as unsafe.
		/// </summary>
		/// <remarks>Useful for testing.</remarks>
		ForcedState = 7,

		/// <summary>
		/// The device is in an error condition.
		/// </summary>
		ErrorCondition = 8,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved1 = 9,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved2 = 10,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved3 = 11,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved4 = 12,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved5 = 13,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved6 = 14,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved7 = 15,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved8 = 16,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved9 = 17,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved10 = 18,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined1 = 19,

        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined2 = 20,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined3 = 21,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined4 = 22,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined5 = 23,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined6 = 24,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined7 = 25,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined8 = 26,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined9 = 27,
        /// <summary>
        /// A user-defined trigger for local use within an observatory.
        /// </summary>
        LocallyDefined10 = 28

    }

}
