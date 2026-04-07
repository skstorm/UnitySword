using System.Collections.Generic;
using GameCore.Data;
using GameCore.Models;
using static GameCore.Data.GameConstants;

namespace GameCore.Logic;

/// <summary>게임 세션의 초기화 및 리셋을 담당하는 유틸리티 클래스.</summary>
internal class GameSessionLogic
{
    /// <summary>최초 실행 시 초기 골드를 지급하고, 나무검 상태로 게임을 시작한다.</summary>
    public static GameState CreateInitialState(PlayerData playerData, SwordDataTable table)
    {
        var data = playerData;
        if (data.IsFirstRun)
        {
            data = data.With(gold: InitialGold, isFirstRun: false);
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

    /// <summary>검이 파괴되었을 때 나무검(+0)으로 리셋한다. 버프와 보호도 초기화.</summary>
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
