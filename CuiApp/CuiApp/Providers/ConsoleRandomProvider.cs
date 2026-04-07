using GameCore.Util;

namespace CuiApp.Providers;

public class ConsoleRandomProvider : IRandomProvider
{
    private readonly Random _rng;
    public int Seed { get; }

    public ConsoleRandomProvider(int? seed = null)
    {
        Seed = seed ?? Environment.TickCount;
        _rng = new Random(Seed);
    }

    public double NextDouble() => _rng.NextDouble();
}
