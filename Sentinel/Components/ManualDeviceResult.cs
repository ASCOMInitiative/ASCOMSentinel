namespace Sentinel.Components;

/// <summary>
/// Result returned by the ManualDeviceDialog when the user clicks OK.
/// </summary>
public record ManualDeviceResult(string Name, string IpAddress, int Port, int DeviceNumber);
