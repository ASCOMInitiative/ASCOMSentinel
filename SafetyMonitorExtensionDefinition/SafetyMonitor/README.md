### ASCOM Safety Monitor ISafetyMonitorV4 Draft Interface Specification
This package contains the draft interface specification for the ASCOM Safety Monitor ISafetyMonitorV4 interface. 

The interface changes are designed to provide a standardized way for safety monitor devices to communicate why they are reporting IsSafe as false.

In addition, the changes enable external clients and devices to register unsafe conditions with the safety monitor thus avoiding the need to 
implement a SafetyMonitor interface themselves.