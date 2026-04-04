using ASCOM.Common.DeviceInterfaces;
using Sentinel;
using Sentinel.DeviceAccess;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;

namespace Sentinel.Tests.DeviceAccess;

public class ObservingConditionsTests
{
    // Short cache window so expiry tests complete in under a second
    private const int CacheWindowMs = 1000;
    private const int CacheExpiryWaitMs = 1500; // Must be > CacheWindowMs

    // Concurrency test parameters
    private const int ConcurrencyTestDurationMs = 10000;
    private const int ReadsPerSecondPerProperty = 1000;
    // With a 100 ms cache window over 1 s the device should be called ~10 times per property.
    // Allow a small margin for start-up and boundary timing.
    private static int MaxDeviceCallsPerProperty => (ConcurrencyTestDurationMs / CacheWindowMs) + 5;

    private readonly Settings _settings;
    private readonly State _state;
    private readonly SentinelLogger _logger;
    private readonly FakeWeatherDevice _fakeDevice;
    private readonly ObservingConditions _sut;

    public ObservingConditionsTests()
    {
        _settings = new Settings();
        _settings.LogLevel = LogLevel.Error; // Suppress log noise during tests
        _settings.PropertyCacheTime = TimeSpan.FromMilliseconds(CacheWindowMs);

        _state = new State();
        _logger = new SentinelLogger(_state, _settings);
        _state.Connected=true;
        _state.Online=true;

        _fakeDevice = new FakeWeatherDevice();

        // Wire every PropertyName to the fake device so the SUT can resolve any property
        foreach (PropertyName property in Enum.GetValues<PropertyName>())
            _state.ObservingConditionsDeviceMap[property] = _fakeDevice;

        _sut = new ObservingConditions(_settings, _state, _logger);
    }

    // -------------------------------------------------------------------------
    // Cache hit — value
    // -------------------------------------------------------------------------

    [Fact]
    public void WhenPropertyReadTwiceWithinCacheWindow_DeviceIsCalledOnce()
    {
        _ = _sut.CloudCover;
        _ = _sut.CloudCover;

        Assert.Equal(1, _fakeDevice.CloudCoverCallCount);
    }

    [Fact]
    public void WhenPropertyReadTwiceWithinCacheWindow_CachedValueIsReturned()
    {
        _fakeDevice.CloudCoverValue = 42.5;
        double first = _sut.CloudCover;

        _fakeDevice.CloudCoverValue = 99.9; // Changed — must NOT be visible within the cache window
        double second = _sut.CloudCover;

        Assert.Equal(first, second);
    }

    // -------------------------------------------------------------------------
    // Cache expiry — value
    // -------------------------------------------------------------------------

    [Fact]
    public void WhenPropertyReadAfterCacheExpiry_DeviceIsCalledAgain()
    {
        _ = _sut.CloudCover;
        Thread.Sleep(CacheExpiryWaitMs);
        _ = _sut.CloudCover;

        Assert.Equal(2, _fakeDevice.CloudCoverCallCount);
    }

    [Fact]
    public void WhenPropertyReadAfterCacheExpiry_UpdatedValueIsReturned()
    {
        _fakeDevice.CloudCoverValue = 10.0;
        _ = _sut.CloudCover;

        Thread.Sleep(CacheExpiryWaitMs);
        _fakeDevice.CloudCoverValue = 75.0;

        Assert.Equal(75.0, _sut.CloudCover);
    }

    // -------------------------------------------------------------------------
    // Cache hit — exception
    // -------------------------------------------------------------------------

    [Fact]
    public void WhenDeviceThrowsException_ExceptionIsCachedAndDeviceIsCalledOnce()
    {
        _fakeDevice.CloudCoverException = new ASCOM.InvalidOperationException("sensor fault");
        try { _ = _sut.CloudCover; } catch { }
        try { _ = _sut.CloudCover; } catch { }

        Assert.Equal(1, _fakeDevice.CloudCoverCallCount);
    }

    [Fact]
    public void WhenDeviceThrowsException_CachedExceptionTypeIsPreserved()
    {
        _fakeDevice.CloudCoverException = new ASCOM.InvalidOperationException("sensor fault");
        try { _ = _sut.CloudCover; } catch { }

        Assert.Throws<ASCOM.InvalidOperationException>(() => _ = _sut.CloudCover);
    }

    [Fact]
    public void WhenDeviceThrowsException_CachedExceptionMessageIsPreserved()
    {
        _fakeDevice.CloudCoverException = new ASCOM.InvalidOperationException("sensor fault");
        try { _ = _sut.CloudCover; } catch { }

        Exception ex = Assert.Throws<ASCOM.InvalidOperationException>(() => _ = _sut.CloudCover);
        Assert.Equal("sensor fault", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Cache expiry — exception
    // -------------------------------------------------------------------------

    [Fact]
    public void WhenDeviceThrowsExceptionAndCacheExpires_DeviceIsCalledAgain()
    {
        _fakeDevice.CloudCoverException = new ASCOM.InvalidOperationException("sensor fault");
        try { _ = _sut.CloudCover; } catch { }

        Thread.Sleep(CacheExpiryWaitMs);
        try { _ = _sut.CloudCover; } catch { }

        Assert.Equal(2, _fakeDevice.CloudCoverCallCount);
    }

    // -------------------------------------------------------------------------
    // Per-property isolation
    // -------------------------------------------------------------------------

    [Fact]
    public void WhenTwoDifferentPropertiesAreReadTwice_EachIsReadOnceFromDevice()
    {
        _ = _sut.CloudCover;
        _ = _sut.Temperature;
        _ = _sut.CloudCover;
        _ = _sut.Temperature;

        Assert.Equal(1, _fakeDevice.CloudCoverCallCount);
        Assert.Equal(1, _fakeDevice.TemperatureCallCount);
    }

    [Fact]
    public void WhenOneCacheExpires_OtherPropertyCacheIsUnaffected()
    {
        _ = _sut.CloudCover;
        _ = _sut.Temperature;

        Thread.Sleep(CacheExpiryWaitMs); // Both caches expire

        _ = _sut.CloudCover;            // Refreshes CloudCover only
        _ = _sut.Temperature;           // Refreshes Temperature independently

        Assert.Equal(2, _fakeDevice.CloudCoverCallCount);
        Assert.Equal(2, _fakeDevice.TemperatureCallCount);
    }

    // -------------------------------------------------------------------------
    // Concurrency — two clients, 500 reads/sec per property
    // -------------------------------------------------------------------------

    // Client 1 reads CloudCover + Temperature; Client 2 reads Humidity + Pressure.
    // Both run for ConcurrencyTestDurationMs at ReadsPerSecondPerProperty each.

    [Fact(Timeout = 15000)]
    public async Task WhenTwoClientsConcurrentlyReadDifferentProperties_NoExceptionsOccur()
    {
        var exceptions = new ConcurrentBag<Exception>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ConcurrencyTestDurationMs));

        var client1 = Task.Run(() => RunReaderLoop(
            [
                () => { try { _ = _sut.CloudCover;  } catch (Exception ex) { exceptions.Add(ex); } },
                () => { try { _ = _sut.Temperature; } catch (Exception ex) { exceptions.Add(ex); } }
            ], cts.Token));
        var client2 = Task.Run(() => RunReaderLoop(
            [
                () => { try { _ = _sut.Humidity; } catch (Exception ex) { exceptions.Add(ex); } },
                () => { try { _ = _sut.Pressure;  } catch (Exception ex) { exceptions.Add(ex); } }
            ], cts.Token));
        await Task.WhenAll(client1, client2);

        Assert.Empty(exceptions);
        await AssertCallRateIsWithinTolerance(await client1);
        await AssertCallRateIsWithinTolerance(await client2);
    }

    [Fact(Timeout = 15000)]
    public async Task WhenTwoClientsConcurrentlyReadDifferentProperties_DeviceCallCountIsBoundedByCache()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ConcurrencyTestDurationMs));

        // At 500 reads/sec per property the cache should absorb the vast majority of calls;
        // the device should only be called once per cache window (~10 times total).
        var client1 = Task.Run(() => RunReaderLoop(
            [() => { _ = _sut.CloudCover; }, () => { _ = _sut.Temperature; }],
            cts.Token));
        var client2 = Task.Run(() => RunReaderLoop(
            [() => { _ = _sut.Humidity; }, () => { _ = _sut.Pressure; }],
            cts.Token));
        await Task.WhenAll(client1, client2);

        Assert.InRange(_fakeDevice.CloudCoverCallCount,  1, MaxDeviceCallsPerProperty);
        Assert.InRange(_fakeDevice.TemperatureCallCount, 1, MaxDeviceCallsPerProperty);
        Assert.InRange(_fakeDevice.HumidityCallCount,    1, MaxDeviceCallsPerProperty);
        Assert.InRange(_fakeDevice.PressureCallCount,    1, MaxDeviceCallsPerProperty);
        await AssertCallRateIsWithinTolerance(await client1);
        await AssertCallRateIsWithinTolerance(await client2);
    }

    [Fact(Timeout = 15000)]
    public async Task WhenTwoClientsConcurrentlyReadDifferentProperties_ReturnedValuesAreConsistent()
    {
        _fakeDevice.CloudCoverValue  = 42.5;
        _fakeDevice.TemperatureValue = 18.5;
        _fakeDevice.HumidityValue    = 65.0;
        _fakeDevice.PressureValue    = 1015.0;

        var inconsistencies = new ConcurrentBag<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ConcurrencyTestDurationMs));

        var client1 = Task.Run(() => RunReaderLoop(
            [
                () => { double v = _sut.CloudCover;  if (v != 42.5)   inconsistencies.Add($"CloudCover={v}");  },
                () => { double v = _sut.Temperature; if (v != 18.5)   inconsistencies.Add($"Temperature={v}"); }
            ], cts.Token));
        var client2 = Task.Run(() => RunReaderLoop(
            [
                () => { double v = _sut.Humidity; if (v != 65.0)   inconsistencies.Add($"Humidity={v}");  },
                () => { double v = _sut.Pressure;  if (v != 1015.0) inconsistencies.Add($"Pressure={v}");  }
            ], cts.Token));
        await Task.WhenAll(client1, client2);

        Assert.Empty(inconsistencies);
        await AssertCallRateIsWithinTolerance(await client1);
        await AssertCallRateIsWithinTolerance(await client2);
    }

    /// <summary>
    /// Runs a tight loop calling each reader action at <see cref="ReadsPerSecondPerProperty"/> iterations/second
    /// until the cancellation token is signalled. Uses Stopwatch + SpinWait for sub-millisecond throttling,
    /// avoiding the ~15 ms OS timer granularity of Task.Delay.
    /// </summary>
    /// <returns>The number of completed iterations and the wall-clock time elapsed.</returns>
    private static (long Iterations, TimeSpan Elapsed) RunReaderLoop(Action[] readers, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        long iterationCount = 0;
        long ticksPerIteration = Stopwatch.Frequency / ReadsPerSecondPerProperty;

        while (!ct.IsCancellationRequested)
        {
            foreach (Action read in readers)
                read();

            // Throttle: spin-wait until the next iteration's scheduled tick
            iterationCount++;
            long nextTick = iterationCount * ticksPerIteration;
            while (sw.ElapsedTicks < nextTick && !ct.IsCancellationRequested)
                Thread.SpinWait(50);
        }

        sw.Stop();
        return (iterationCount, sw.Elapsed);
    }

    /// <summary>
    /// Asserts that the actual iteration rate from <paramref name="result"/> is within
    /// <paramref name="toleranceFraction"/> of <see cref="ReadsPerSecondPerProperty"/>.
    /// </summary>
    private async static Task AssertCallRateIsWithinTolerance(
        (long Iterations, TimeSpan Elapsed) result,
        double toleranceFraction = 0.1)
    {
        double actualRate = result.Iterations / result.Elapsed.TotalSeconds;
        Assert.InRange(
            actualRate,
            ReadsPerSecondPerProperty * (1 - toleranceFraction),
            ReadsPerSecondPerProperty * (1 + toleranceFraction));
    }
}

/// <summary>
/// Minimal test double for <see cref="IObservingConditionsV2"/> that tracks call counts
/// and supports configurable return values and exceptions for CloudCover and Temperature.
/// </summary>
internal sealed class FakeWeatherDevice : IObservingConditionsV2
{
    public int CloudCoverCallCount { get; private set; }
    public double CloudCoverValue { get; set; } = 50.0;
    public Exception? CloudCoverException { get; set; }

    public int TemperatureCallCount { get; private set; }
    public double TemperatureValue { get; set; } = 20.0;
    public Exception? TemperatureException { get; set; }

    public double CloudCover
    {
        get
        {
            CloudCoverCallCount++;
            if (CloudCoverException is not null) throw CloudCoverException;
            return CloudCoverValue;
        }
    }

    public double Temperature
    {
        get
        {
            TemperatureCallCount++;
            if (TemperatureException is not null) throw TemperatureException;
            return TemperatureValue;
        }
    }

    // Remaining interface members — not under test
    public bool Connecting { get; set; }
    public bool Connected { get; set; }=true;
    public List<StateValue> DeviceState => [];
    public double AveragePeriod { get; set; }
    public double DewPoint => throw new NotImplementedException();
    public int HumidityCallCount { get; private set; }
    public double HumidityValue { get; set; } = 60.0;
    public Exception? HumidityException { get; set; }

    public int PressureCallCount { get; private set; }
    public double PressureValue { get; set; } = 1013.25;
    public Exception? PressureException { get; set; }

    public double Humidity
    {
        get
        {
            HumidityCallCount++;
            if (HumidityException is not null) throw HumidityException;
            return HumidityValue;
        }
    }

    public double Pressure
    {
        get
        {
            PressureCallCount++;
            if (PressureException is not null) throw PressureException;
            return PressureValue;
        }
    }
    public double RainRate => throw new NotImplementedException();
    public double SkyBrightness => throw new NotImplementedException();
    public double SkyQuality => throw new NotImplementedException();
    public double StarFWHM => throw new NotImplementedException();
    public double SkyTemperature => throw new NotImplementedException();
    public double WindDirection => throw new NotImplementedException();
    public double WindGust => throw new NotImplementedException();
    public double WindSpeed => throw new NotImplementedException();
    public string Description => string.Empty;
    public string DriverInfo => string.Empty;
    public string DriverVersion => string.Empty;
    public short InterfaceVersion => 2;
    public string Name => string.Empty;
    public IList<string> SupportedActions => [];
    public string Action(string actionName, string actionParameters) => throw new NotImplementedException();
    public void CommandBlind(string command, bool raw = false) => throw new NotImplementedException();
    public bool CommandBool(string command, bool raw = false) => throw new NotImplementedException();
    public string CommandString(string command, bool raw = false) => throw new NotImplementedException();
    public void Connect() { }
    public void Disconnect() { }
    public void Dispose() { }
    public void Refresh() { }
    public string SensorDescription(string PropertyName) => string.Empty;
    public double TimeSinceLastUpdate(string PropertyName) => 0;
}
