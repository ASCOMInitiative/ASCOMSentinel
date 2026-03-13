# ASCOM Sentinel
This cross-platform application:
* Consolidates a number of COM or Alpaca ObservingConditions devices into a single virtual device.
* Consolidates a number of COM or Alpaca SafetyMonitor devices into a single virtual device.
* Allows the user to define rules for each virtual device ObservingConditions property that can trigger the the virtual safety monitor to report IsSafe as false.
* Employs cacheing on virtual device properties to support heavy loads efficiently while protecting backend devices.
* The virtual safety monitor provides a GetSafetyState Action that, when IsSafe as false, returns a JSON serialised object contining a list of causes.

**At the time of release (March 2026) this application should be treated as experimental and used with care.**

**Any feeback on features or operation (positivce as well as negative) would be appreciated, please send this as a GitHub Issue.**
