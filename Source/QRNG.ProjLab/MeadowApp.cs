using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dotnetMakers;

public class MeadowApp : ProjectLabCoreComputeApp
{
    private DisplayService _display;
    private IAnalogInputArray _adc;
    private Randomizer _randomizer;
    private Random _random;
    private QRNGProfiler _profiler;

    public override Task Initialize()
    {
        _display = new DisplayService(Hardware.Display);
        _adc = Hardware.ComputeModule.CreateAnalogInputArray(Hardware.MikroBus1.Pins.AN);

        _random = new Random();

        _randomizer = new Randomizer(_adc);
        _randomizer.AutoFindCenterVoltage();
        Resolver.Log.Info($"Center voltage: {_randomizer.CenterVoltage.Volts:N3}V");

        _profiler = new QRNGProfiler(_randomizer);

        Hardware.LeftButton.Clicked += OnLeftButtonClicked;
        Hardware.RightButton.Clicked += OnRightButtonClicked;
        Hardware.UpButton.Clicked += OnUpButtonClicked;

        return base.Initialize();
    }

    private void OnLeftButtonClicked(object? sender, EventArgs e)
    {
        _profiler.Profile();
    }

    private void OnUpButtonClicked(object? sender, EventArgs e)
    {
        Resolver.Log.Info($"Creating pseudorandom noise...");

        var sw = Stopwatch.StartNew();

        var data = new byte[320 * 240];
        _random.NextBytes(data);

        sw.Stop();
        Resolver.Log.Info($"Getting {data.Length} bytes took {sw.ElapsedMilliseconds}ms");

        _display.SetNoisePattern(data);
    }

    private void OnRightButtonClicked(object? sender, EventArgs e)
    {
        Resolver.Log.Info($"Creating noise...");

        var sw = Stopwatch.StartNew();

        var data = new byte[320 * 240];
        _randomizer.NextBytes(data);
        //Random.Shared.NextBytes(data);

        sw.Stop();
        Resolver.Log.Info($"Getting {data.Length} bytes took {sw.ElapsedMilliseconds}ms");

        _display.SetNoisePattern(data);
    }
}