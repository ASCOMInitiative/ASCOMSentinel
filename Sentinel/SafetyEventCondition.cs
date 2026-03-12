namespace Sentinel
{
    /// <summary>
    /// Specifies the nature of the safety rule that triggered this event.
    /// </summary>
    /// <remarks>
    /// Please choose the value that gives the most information about the state of the property. If you have conditions that require other states, please request them on the:
    /// <see href="https://ascomtalk.groups.io/g/Developer/topics">ASCOM Developers Forum</see>.
    /// </remarks>
    public enum SafetyEventCondition
    {
        /// <summary>
        /// The property has fallen below the safety threshold defined for this property.
        /// </summary>
        /// <remarks>Only for ObservingConditions devices.</remarks>
        BelowLimit,

        /// <summary>
        /// The property has reached the safety threshold defined for this property.
        /// </summary>
        /// <remarks>Only for ObservingConditions devices.</remarks>
        EqualLimit,

        /// <summary>
        /// The property has exceeded the safety threshold defined for this property.
        /// </summary>
        /// <remarks>Only for ObservingConditions devices.</remarks>
        AboveLimit,

        /// <summary>
        /// The property is in an unsafe state.
        /// </summary>
        /// <remarks>Only for SafetyMonitor devices.</remarks>
        Unsafe,

        /// <summary>
        /// The property has been forced to a specific state or value.
        /// </summary>
        /// <remarks>For all devices.</remarks>
        ForcedToState,

        /// <summary>
        /// The device is in an error state.
        /// </summary>
        /// <remarks>For all devices.</remarks>
        DeviceInErrorState,

        /// <summary>
        /// The property is not available.
        /// </summary>
        /// <remarks>Never return this value from a device, it is only included for display purposes. Use one of the other values to indicate the state of the property.</remarks>
        NotAvailable
    }
}
