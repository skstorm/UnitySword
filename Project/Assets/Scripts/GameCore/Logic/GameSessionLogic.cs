using System.Collections.Generic;
using GameCore.Data;
using GameCore.Models;

namespace GameCore.Logic;

internal class GameSessionLogic
{
    public static GameState CreateInitialState(PlayerData playerData, SwordDataTable table)
    {
        var data = playerData;
        if (data.IsFirstRun)
        {
            data = data.With(gold: 200, isFirstRun: false);
        }

        var woodenSword = table.GetSword(0);
        return new GameState(
            currentSword: woodenSword,
            currentLevel: 0,
            playerData: data,
            activeModifiers: new List<IModifier>(),
            hasActiveProtection: false,
            pendingAdProtection: false);
    }

    public static GameState ResetToWoodenSword(GameState state, SwordDataTable table)
    {
        var woodenSword = table.GetSword(0);
        return state.With(
            currentSword: woodenSword,
            currentLevel: 0,
            activeModifiers: new List<IModifier>(),
            hasActiveProtection: false,
            pendingAdProtection: false);
    }
}
