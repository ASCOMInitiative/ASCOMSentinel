namespace Sentinel
{
    /// <summary>
    /// Per-circuit (per-browser-tab) state that survives page navigation within
    /// the same Blazor circuit but is isolated between different browsers.
    /// Registered as a scoped service so each circuit gets its own instance.
    /// </summary>
    public class PerBrowserState
    {
        private bool _isAuthenticated;
        private bool _mustChangePassword;

        /// <summary>
        /// Whether this browser tab has authenticated as an administrator.
        /// </summary>
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; AuthStateChanged?.Invoke(); }
        }

        /// <summary>
        /// Whether the administrator must change the password before proceeding.
        /// </summary>
        public bool MustChangePassword
        {
            get => _mustChangePassword;
            set { _mustChangePassword = value; AuthStateChanged?.Invoke(); }
        }

        /// <summary>
        /// Fired when <see cref="IsAuthenticated"/> changes so components
        /// in the same circuit (e.g. NavMenu) can re-render.
        /// </summary>
        public event Action? AuthStateChanged;
    }
}
