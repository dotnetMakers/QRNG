using Meadow;
using System;
using System.Diagnostics;

namespace dotnetMakers;

public class QRNGProfiler
{
    private readonly Randomizer _randomizer;
    private readonly int _iterations;

    public QRNGProfiler(Randomizer randomizer, int interationsForProfiling = 100)
    {
        _randomizer = randomizer;
        _iterations = interationsForProfiling;
    }

    public void Profile()
    {
        Resolver.Log.Info($"Profiling...");

        var adcQuery = GetAdcQueryTime();
        Resolver.Log.Info($"ADC Queries take: {adcQuery.TotalMilliseconds:N3}ms");

        var byteTime = GetByteTime();
        Resolver.Log.Info($"1 byte gen takes: {byteTime.TotalMilliseconds:N3}ms");

        var intTime = GetIntTime();
        Resolver.Log.Info($"1 uint gen takes: {intTime.TotalMilliseconds:N3}ms");

        var kTime = Get1kTime();
        Resolver.Log.Info($"1024 bytes takes: {kTime.TotalMilliseconds:N3}ms");
    }

    private TimeSpan GetAdcQueryTime()
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < _iterations; i++)
        {
            _randomizer.ReadAdcVolts();
        }

        sw.Stop();

        return TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / _iterations);
    }

    private TimeSpan GetByteTime()
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < _iterations; i++)
        {
            _randomizer.GetRandomByte();
        }

        sw.Stop();

        return TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / _iterations);
    }

    private TimeSpan GetIntTime()
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < _iterations; i++)
        {
            _randomizer.GetRandomUInt32();
        }

        sw.Stop();

        return TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / _iterations);
    }

    private TimeSpan Get1kTime()
    {
        var buffer = new byte[1024];

        var sw = Stopwatch.StartNew();

        _randomizer.NextBytes(buffer);

        sw.Stop();

        return TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds / _iterations);
    }
}
