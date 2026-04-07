using GameCore.Data;
using GameCore.Util;

namespace GameCore.Engine;

public class GameContext
{
    public IRandomProvider Random { get; }
    public ITimeProvider Time { get; }
    public SwordDataTable SwordTable { get; }
    public bool IsAdSystemEnabled { get; }

    public GameContext(IRandomProvider random, ITimeProvider time, SwordDataTable swordTable, bool isAdSystemEnabled = false)
    {
        Random = random;
        Time = time;
        SwordTable = swordTable;
        IsAdSystemEnabled = isAdSystemEnabled;
    }
}
