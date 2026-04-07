using System;
using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>
/// 강화 커맨드. 골드를 소모하여 검의 레벨을 올리는 핵심 게임 루프.
/// 실패 시 검이 파괴되고 나무검으로 리셋된다 (보호 부적 제외).
/// </summary>
public class EnhanceCommand : Command
{
    /// <summary>광고 보호 대기 중, 최대 레벨, 골드 부족 여부를 검증.</summary>
    public override string Validate(GameState state, GameContext context)
    {
        if (state.PendingAdProtection)
            return "pending_ad_protection";

        var nextLevel = state.CurrentLevel + 1;
        var targetSword = context.SwordTable.GetSword(nextLevel);
        if (targetSword == null)
            return "max_level_reached";

        if (state.PlayerData.Gold < targetSword.EnhanceCost)
            return "insufficient_gold";

        return null;
    }

    /// <summary>
    /// 강화 실행 흐름: 골드 차감 → 숙련도 경험치 → 성공/실패 판정 → 통계 갱신.
    /// </summary>
    public override CommandResult Execute(GameState state, GameContext context)
    {
        var enhanceLogic = context.EnhanceLogic;
        var economyLogic = context.EconomyLogic;

        var nextLevel = state.CurrentLevel + 1;
        var targetSword = context.SwordTable.GetSword(nextLevel);

        // 1) 강화 비용 차감
        var spendResult = economyLogic.SpendGold(state, targetSword.EnhanceCost);
        var currentState = spendResult.NewState;
        var allEvents = new List<GameEvent>(spendResult.Events);

        // 2) 숙련도 경험치 지급 (높은 레벨일수록 더 많이)
        currentState = GrantMasteryExp(currentState, state.CurrentLevel);

        // 3) 성공/실패 판정
        var effectiveRate = enhanceLogic.GetEffectiveRate(currentState, targetSword);
        var success = enhanceLogic.Roll(effectiveRate, context.Random);

        // 4) 결과 처리 및 통계 갱신
        var stats = currentState.PlayerData.Stats;
        if (success)
        {
            currentState = HandleSuccess(currentState, targetSword, context, enhanceLogic, allEvents, stats, nextLevel);
        }
        else
        {
            currentState = HandleFailure(currentState, state, context, enhanceLogic, allEvents, stats);
        }

        return new CommandResult(currentState, allEvents);
    }

    /// <summary>숙련도 경험치 지급. 현재 레벨이 높을수록 더 많은 경험치.</summary>
    private static GameState GrantMasteryExp(GameState state, int currentLevel)
    {
        var gain = 1 + currentLevel;
        var newExp = state.PlayerData.MasteryExp + gain;
        return state.With(playerData: state.PlayerData.With(masteryExp: newExp));
    }

    /// <summary>강화 성공: 검 레벨 상승 + 연속 성공 통계 갱신.</summary>
    private static GameState HandleSuccess(
        GameState currentState, Sword targetSword, GameContext context,
        EnhanceLogic enhanceLogic, List<GameEvent> allEvents, Statistics stats, int nextLevel)
    {
        var successResult = enhanceLogic.HandleSuccess(currentState, targetSword, context.SwordTable);
        currentState = successResult.NewState;
        allEvents.AddRange(successResult.Events);

        var newConsecutiveSuccess = stats.CurrentConsecutiveSuccess + 1;
        var newStats = stats.With(
            totalEnhances: stats.TotalEnhances + 1,
            highestEnhance: Math.Max(stats.HighestEnhance, nextLevel),
            currentConsecutiveSuccess: newConsecutiveSuccess,
            maxConsecutiveSuccess: Math.Max(stats.MaxConsecutiveSuccess, newConsecutiveSuccess),
            currentConsecutiveFail: 0);

        return currentState.With(playerData: currentState.PlayerData.With(stats: newStats));
    }

    /// <summary>강화 실패: 파괴 여부 판단 → 파편 지급 → 나무검 리셋 + 연속 실패 통계 갱신.</summary>
    private static GameState HandleFailure(
        GameState currentState, GameState originalState, GameContext context,
        EnhanceLogic enhanceLogic, List<GameEvent> allEvents, Statistics stats)
    {
        var failResult = enhanceLogic.HandleFail(currentState, context.SwordTable, context);
        currentState = failResult.NewState;
        allEvents.AddRange(failResult.Events);

        // 보호 부적도 없고 광고 보호 대기도 아니면 → 파괴
        var destroyed = !originalState.HasActiveProtection && !currentState.PendingAdProtection;

        var newConsecutiveFail = stats.CurrentConsecutiveFail + 1;
        var newStats = stats.With(
            totalEnhances: stats.TotalEnhances + 1,
            totalDestroys: destroyed ? stats.TotalDestroys + 1 : stats.TotalDestroys,
            currentConsecutiveFail: newConsecutiveFail,
            maxConsecutiveFail: Math.Max(stats.MaxConsecutiveFail, newConsecutiveFail),
            currentConsecutiveSuccess: 0);

        currentState = currentState.With(playerData: currentState.PlayerData.With(stats: newStats));

        // 파괴 시 파편 지급 후 나무검으로 리셋
        if (destroyed)
        {
            currentState = ApplyDestruction(currentState, originalState, context);
        }

        return currentState;
    }

    /// <summary>검 파괴 처리: 파편 보상 지급 + 나무검 리셋.</summary>
    private static GameState ApplyDestruction(GameState currentState, GameState originalState, GameContext context)
    {
        var fragments = originalState.CurrentSword.FragmentReward;
        if (fragments > 0)
        {
            var newFragments = currentState.PlayerData.Fragments + fragments;
            currentState = currentState.With(playerData: currentState.PlayerData.With(fragments: newFragments));
        }
        return GameSessionLogic.ResetToWoodenSword(currentState, context.SwordTable);
    }
}
