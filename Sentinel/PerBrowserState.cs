namespace Sentinel
{
    /// <summary>
    /// Per-circuit (per-browser-tab) state that survives page navigation within
    /// the same Blazor circuit but is isolated between different browsers.
    /// Registered as a scoped service so each circuit gets its own instance.
    /// </summary>
    public class PerBrowserState
    {

    }
}
