using GameCore.Data;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class GameSessionLogic
    {
        public GameState CreateInitialState(PlayerData playerData, SwordDataTable swordTable)
        {
            var woodenSword = swordTable.GetSword(0);
            var data = playerData.IsFirstRun
                ? playerData.With(gold: 200, isFirstRun: false)
                : playerData;
            return new GameState(
                currentSword: woodenSword,
                currentLevel: 0,
                playerData: data,
                hasActiveProtection: false,
                pendingAdProtection: false);
        }

        public GameState ResetToWoodenSword(GameState state, SwordDataTable swordTable)
        {
            return state.With(
                currentSword: swordTable.GetSword(0),
                currentLevel: 0,
                hasActiveProtection: false,
                pendingAdProtection: false);
        }
    }
}
