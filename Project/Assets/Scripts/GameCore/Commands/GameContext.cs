using GameCore.Data;
using GameCore.Util;

namespace GameCore.Commands
{
    public class GameContext
    {
        public IRandomProvider Random { get; }
        public ITimeProvider Time { get; }
        public SwordDataTable SwordTable { get; }

        public GameContext(IRandomProvider random, ITimeProvider time, SwordDataTable swordTable)
        {
            Random = random;
            Time = time;
            SwordTable = swordTable;
        }
    }
}
