# ASCOM Sentinel
Ideally, ASCOM client applications need a single source of consolidated ObservingConditions and SafetyMonitor information on which to base decisions about whether it is safe to continue observing, 
and it is common for astronomers to have multiple devices exposing ObservingConditions and SafetyMonitor ASCOM interfaces from different manufacturers.

**ASCOM Sentinel** is a cross-platform application that provides:
- A consolidated observing condition device that aggregates multiple ObservingConditions devices
- A consolidated safety monitor device that aggregates multiple SafetyMonitor devices. 
 
Conceptually, ASCOM Sentinel is similar to the Observing Conditions Hub (OCH) that is part of the ASCOM Platform, but with a number of important differences, 
particularly that it is an Alpaca device and includes a built-in safety monitor device.

ASCOM Sentinel improves on OCH capabilities by enabling astronomers to define rules based on observing conditions values, e.g. maximum allowed wind-speed, which will 
trigger the consolidated safety monitor to report an unsafe condition. In addition it will make the reason(s) for the unsafe state available through 
the proposed extension to the safety monitor interface: [Safety Monitor Interface Proposal](https://ascom-standards.org/isafetymonitorv4).

## Important
**This application is currently considered experimental and should only be used in conjunction with independent safety mechanics that will assure human and equipment safety.**

## ASCOM Sentinel Features
- Alpaca native with no requirement for ASCOM Platform installation on Linux and macOS.
- Available for: Windows (x64, ARM64), Linux (ARM32, ARM64, x64) and macOS (x64, ARM64).
- Consolidates a number of real Alpaca or COM ObservingConditions devices into a single virtual device.
- Consolidates a number of real Alpaca or COM SafetyMonitor devices into a single virtual device.
- Allows the user to define rules for each virtual device ObservingConditions property that will trigger the virtual safety monitor to report <code>IsSafe</code> as false. 
- Employs configurable caching on virtual device properties to support heavy client loads and protect back-end devices.
- Provides an optional two-level user / administrator security model to support use in community environments.
- Provides a graphical depiction of current observing conditions values as reported by the consolidated device.
- The consolidated devices can be accessed from COM clients by creating [Dynamic Drivers](https://www.ascom-standards.org/help/html/e3870a2f-582a-4ab4-b37f-e9b1c37a2030.htm) using the Platform Chooser.
- Supports draft specifications in the [Safety Monitor Interface Proposal](https://ascom-standards.org/isafetymonitorv4) by implementing the 
following <code>SafetyMonitor.SupportedActions</code> / <code>SafetyMonitor.Action</code> actions:
    - <code>SafetyEvents</code> - Returns a set of reasons identifying why the safety monitor reports <code>IsSafe</code> as false.
    - <code>SetExternalEvents</code> - Enables clients and devices to add their own  safety events to the list returned by the <code>SafetyEvents</code> action.
    - <code>ClearExternalEvents</code> -Enables clients and devices to clear their own safety events from the list.

## Feedback and Issues
All feedback on features, usefulness and operation is welcome, please post this on the [ASCOM Developer's forum](https://ascomtalk.groups.io/g/Developer/topics).
If you experience problems with the application please raise them on [GitHub Issues](https://github.com/ASCOMInitiative/ASCOMSentinel/issues).