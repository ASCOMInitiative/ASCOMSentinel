using System;
using System.Collections.Generic;
using System.Text;

namespace SafetyMonitorInterfaceProposal
{
    /// <summary>
    /// Proposed interface additions to the SafetyMonitorP interface that can be used to track safety events.
    /// </summary>
    public class SafetyMonitorInterface
    {
        /// <summary>
        /// Returns a list of safety state events
        /// </summary>
        /// <returns>
        /// <para>COM: An ArrayList of SafetyState objects representing the current safety state.</para>
        /// <para>Alpaca: a JSON array of SafetyState objects representing the current safety state.</para>
        /// </returns>
        public IEnumerable<SafetyState> GetSafetyState()
        {
            return [];
        }

        /// <summary>
        /// Adds a list of safety events to the current safety state. If an event already exists, it will be updated with the new information.
        /// </summary>
        /// <param name="safetyState">A list of safety states to be added to the current safety state.</param>
        public void SetSafetyEvent(IEnumerable<SafetyState> safetyState) { }

        /// <summary>
        /// Clears a list of safety events from the current safety state. If an event does not exist, it will be ignored.
        /// </summary>
        /// <param name="safetyState">A list of safety states to be cleared from the current safety state.</param>
        /// <remarks>Only the RuleId field is used in determining a match to existing safety states.</remarks>
        public void ClearSafetyEvent(IEnumerable<SafetyState> safetyState) { }
    }
}
