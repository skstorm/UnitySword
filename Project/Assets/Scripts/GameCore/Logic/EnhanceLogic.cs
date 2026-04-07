using System;
using System.Collections.Generic;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using GameCore.Commands;
using GameCore.Util;

namespace GameCore.Logic;

/// <summary>강화 판정 및 성공/실패 처리를 담당하는 로직 클래스.</summary>
internal class EnhanceLogic
{
    /// <summary>활성 버프(IModifier)를 모두 적용한 최종 성공률을 계산한다.</summary>
    public double GetEffectiveRate(GameState state, Sword targetSword)
    {
        double rate = targetSword.SuccessRate;
        foreach (var mod in state.ActiveModifiers)
        {
            rate = mod.Apply(rate);
        }
        return Math.Clamp(rate, 0.0, 1.0);
    }

    /// <summary>성공률을 기반으로 강화 성공 여부를 판정한다.</summary>
    public bool Roll(double effectiveRate, IRandomProvider random)
    {
        return random.NextDouble() < effectiveRate;
    }

    /// <summary>강화 성공 시 검 레벨을 올리고 버프를 초기화한다.</summary>
    public CommandResult HandleSuccess(GameState state, Sword newSword, SwordDataTable table)
    {
        var newLevel = state.CurrentLevel + 1;
        var newState = state.With(
            currentSword: newSword,
            currentLevel: newLevel,
            activeModifiers: new List<IModifier>());

        // 다음 단계 강화 비용을 이벤트에 포함 (UI 표시용)
        var nextSword = table.GetSword(newLevel + 1);
        var nextCost = nextSword?.EnhanceCost ?? 0;

        var events = new List<GameEvent>
        {
            new EnhanceSuccessEvent(state.CurrentLevel, newLevel, newSword.Name, nextCost)
        };

        return new CommandResult(newState, events);
    }

    /// <summary>
    /// 강화 실패 처리. 보호 부적이 있으면 검을 지키고,
    /// 광고 보호가 가능하면 대기 상태로, 아니면 파괴 이벤트를 발생시킨다.
    /// </summary>
    public CommandResult HandleFail(GameState state, SwordDataTable table, GameContext context)
    {
        // 보호 부적 활성 시 — 검 보존, 부적 소모
        if (state.HasActiveProtection)
        {
            var ns = state.With(
                hasActiveProtection: false,
                activeModifiers: new List<IModifier>());

            var events = new List<GameEvent>
            {
                new EnhanceFailEvent(
                    state.CurrentLevel,
                    state.CurrentSword.Name,
                    state.CurrentSword.SellPrice,
                    0,
                    false,
                    false)
            };

            return new CommandResult(ns, events);
        }

        // 광고 보호 가능 여부 확인 (Unity 전용 기능)
        var adAvailable = context.IsAdSystemEnabled && state.PlayerData.AdLimits.CanUseAdProtection();
        var newState = state.With(
            pendingAdProtection: adAvailable,
            activeModifiers: new List<IModifier>());

        var failEvents = new List<GameEvent>
        {
            new EnhanceFailEvent(
                state.CurrentLevel,
                state.CurrentSword.Name,
                state.CurrentSword.SellPrice,
                state.CurrentSword.FragmentReward,
                adAvailable,
                true)
        };

        return new CommandResult(newState, failEvents);
    }
}
