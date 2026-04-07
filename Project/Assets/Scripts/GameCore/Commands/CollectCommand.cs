using System.Collections.Generic;
using System.Linq;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>현재 검을 컬렉션에 등록하고 나무검으로 리셋하는 커맨드.</summary>
public class CollectCommand : Command
{
    /// <summary>수집 가능 조건: 최소 레벨, 수집 가능 검, 중복 등록 방지.</summary>
    public override string Validate(GameState state, GameContext context)
    {
        if (state.PendingAdProtection) return "pending_ad_protection";
        if (state.CurrentLevel < GameConstants.MinCollectionLevel) return "level_too_low";
        if (!state.CurrentSword.Collectable) return "not_collectable";
        if (state.PlayerData.CollectedSwords.Contains(state.CurrentLevel))
            return "already_collected";
        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var events = new List<GameEvent>();
        var sword = state.CurrentSword;
        var level = state.CurrentLevel;

        events.Add(new CollectEvent(sword.Name, level));

        // 컬렉션에 현재 레벨 추가
        var collected = new List<int>(state.PlayerData.CollectedSwords) { level };
        var newPlayerData = state.PlayerData.With(collectedSwords: collected);
        var stateWithCollection = state.With(playerData: newPlayerData);

        // 수집 후 나무검으로 리셋
        var newState = GameSessionLogic.ResetToWoodenSword(stateWithCollection, context.SwordTable);

        return new CommandResult(newState, events);
    }
}
