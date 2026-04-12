namespace SafetyMonitor
{
    /// <summary>
    /// Proposed interface additions to the SafetyMonitor interface that can be used to track safety events.
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
        /// <para>The returned list must only contain entries with unique <see cref="SafetyEvent.Id" /> values.</para>
        /// <para>See <see href="GetSafetyEventsImplementation.htm">Implementation Notes</see> for information on how clients, Alpaca devices and drivers should implement this method.</para>
        /// </remarks>
        public IEnumerable<SafetyEvent> SafetyEvents { get; }

        /// <summary>
        /// Adds a list of <see cref="SafetyEvent"/> objects to the list returned by <see cref="SafetyEvents"/>.
        /// </summary>
        /// <param name="safetyEventList">An enumerable list of <see cref="SafetyEvent"/> objects to be added to the current safety event list.</param>
        /// <remarks> If an event already exists in the safety monitor's external events list, based on its EventId, the stored value will be updated with the revised information. 
        /// i.e. the list returned by <see cref="SafetyEvents"/> will not contain entries with duplicate <see cref="SafetyEvent.Id"/> values.</remarks>
        public void SetExternalEvents(IEnumerable<SafetyEvent> safetyEventList);

        /// <summary>
        /// Removes a list of <see cref="SafetyEvent"/> objects from the list returned by <see cref="SafetyEvents"/>.
        /// </summary>
        /// <exception cref="ASCOM.InvalidValueException">One or more safety event ids are not recognised.</exception>
        /// <param name="safetyEventIdList">An enumerable list of <see cref="SafetyEvent.Id">safety event ID strings</see> to be cleared from the current safety event list.</param>
        public void ClearExternalEvents(IEnumerable<string> safetyEventIdList);
    }
}
