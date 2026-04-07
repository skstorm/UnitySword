using GameCore.Data;
using GameCore.Engine;
using GameCore.Logic;
using GameCore.Models;
using GameCore.Util;

namespace CuiApp.Tests;

/// <summary>Fixed random provider for deterministic tests.</summary>
public class FixedRandomProvider : IRandomProvider
{
    private readonly Queue<double> _values = new();

    public FixedRandomProvider(params double[] values)
    {
        foreach (var v in values) _values.Enqueue(v);
    }

    public double NextDouble() => _values.Count > 0 ? _values.Dequeue() : 0.5;
}

public class FixedTimeProvider : ITimeProvider
{
    public DateTime Now() => new(2026, 1, 1);
}

public static class TestSetup
{
    public static SwordDataTable LoadSwordTable()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "swords.csv");
        return SwordDataLoader.Load(csvPath);
    }

    public static GameContext CreateContext(params double[] randomValues)
    {
        var random = new FixedRandomProvider(randomValues);
        var time = new FixedTimeProvider();
        var table = LoadSwordTable();
        return new GameContext(random, time, table, isAdSystemEnabled: false);
    }

    public static (GameEngine engine, GameContext context) CreateGame(params double[] randomValues)
    {
        var context = CreateContext(randomValues);
        var playerData = PlayerData.CreateDefault();
        var state = GameSessionLogic.CreateInitialState(playerData, context.SwordTable);
        var engine = new GameEngine(state, context);
        return (engine, context);
    }
}
