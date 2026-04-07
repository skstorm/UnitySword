using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class DestroyFinalizationLogic
    {
        public LogicResult Finalize(GameState state, int destroyedLevel, string destroyedName,
            SwordDataTable swordTable, MasteryDataTable masteryTable)
        {
            var events = new List<GameEvent>();
            var s = state;

            // Fragment reward (base from CSV, bonus from mastery)
            var fragmentBonus = new MasteryLogic().GetFragmentBonus(s, masteryTable);
            var fragResult = new FragmentLogic().GiveFragments(s, destroyedLevel,
                fragmentBonus, swordTable);
            s = fragResult.NewState;
            events.AddRange(fragResult.Events);

            // Reset to wooden sword
            s = new GameSessionLogic().ResetToWoodenSword(s, swordTable);
            events.Add(new DestroyConfirmedEvent(destroyedLevel, destroyedName));

            return new LogicResult(s, events);
        }
    }
}
