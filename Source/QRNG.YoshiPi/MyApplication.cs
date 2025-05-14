using Meadow;
using Meadow.Hardware;
using System.Diagnostics;
using YoshiPi;

namespace dotnetMakers;

internal sealed class MyApplication : YoshiPiApp
{
    private DisplayService _display;
    private IAnalogInputPort _adc;
    private Randomizer _randomizer;
    private QRNGProfiler _profiler;

    public override Task Initialize()
    {
        _display = new DisplayService(Hardware.Display);
        _adc = Hardware.Adc.Pins.A00.CreateAnalogInputPort(1);

        _randomizer = new Randomizer(_adc);
        _randomizer.AutoFindCenterVoltage();
        Resolver.Log.Info($"Center voltage: {_randomizer.CenterVoltage.Volts:N3}V");

        _profiler = new QRNGProfiler(_randomizer);

        Hardware.Button1.Clicked += OnButton1Clicked;
        Hardware.Button2.Clicked += OnButton2Clicked;

        return base.Initialize();
    }

    private void OnButton1Clicked(object? sender, EventArgs e)
    {
        _profiler.Profile();
    }

    private void OnButton2Clicked(object? sender, EventArgs e)
    {
        Resolver.Log.Info($"Creating noise...");

        var sw = Stopwatch.StartNew();

        var data = new byte[320 * 240];
        //_randomizer.NextBytes(data);
        Random.Shared.NextBytes(data);

        sw.Stop();
        Resolver.Log.Info($"Getting {data.Length} bytes took {sw.ElapsedMilliseconds}ms");

        _display.SetNoisePattern(data);
    }

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }
}
