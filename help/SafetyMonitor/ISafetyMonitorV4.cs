namespace SafetyMonitor
{
    /// <summary>
    /// Proposed interface additions to the SafetyMonitorP interface that can be used to track safety events.
    /// </summary>
    public interface ISafetyMonitorV4
    {
        /// <summary>
        /// Returns a list of safety events
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="SafetyEvent"/> objects. Must return an empty collection when no events are active (must not return null).
        /// </returns>
        /// <remarks>
        /// <para>The returned list must only contain entries with unique <see cref="SafetyEvent.EventId" /> values.</para>
        /// <para>See <see href="GetSafetyEventsImplementation.htm">Implementation Notes</see> for information on how clients, Alpaca devices and drivers should implement this method.</para>
        /// </remarks>
        public IEnumerable<SafetyEvent> SafetyEvents { get; }

        /// <summary>
        /// Adds <see cref="SafetyEvent"/> objects to the list returned by the <see cref="SafetyEvents"/> property.
        /// </summary>
        /// <param name="safetyEvents">A list of safety states to be added to the current safety state.</param>
        /// <remarks> If an event already exists in the safety monitor's external events list, based on its EventId, the stored value will be updated with the revised information. 
        /// i.e. the list returned by <see cref="SafetyEvents"/> must not contain duplicate values.</remarks>
        public void SetExternalEvents(IEnumerable<SafetyEvent> safetyEvents);

        /// <summary>
        /// Clears a list of safety events from the current safety state. If an event does not exist, it will be ignored.
        /// </summary>
        /// <exception cref="ASCOM.InvalidValueException">One or more safety event ids are not recognised.</exception>
        /// <param name="safetyEventIds">A list of safety event IDs to be cleared from the current safety state.</param>
        public void ClearExternalEvents(IEnumerable<string> safetyEventIds);
    }
}
