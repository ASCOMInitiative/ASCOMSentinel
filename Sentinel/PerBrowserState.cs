namespace Sentinel
{
    /// <summary>
    /// Per-circuit (per-browser-tab) state that survives page navigation within
    /// the same Blazor circuit but is isolated between different browsers.
    /// Registered as a scoped service so each circuit gets its own instance.
    /// </summary>
    public class PerBrowserState
    {
        /// <summary>
        /// Gets or sets the index of the first visible log entry in the log view.
        /// </summary>
        public int TopOfVisibleLog { get; set; }

        /// <summary>
        /// Gets or sets the number of lines in the screen log when last updated.
        /// </summary>
        public int LastNumberOfLogLines { get; set; }
    }
}
