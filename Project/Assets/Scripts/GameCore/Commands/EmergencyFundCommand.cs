using GameCore.Data;
using GameCore.Engine;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>
/// 긴급 지원금 커맨드. 나무검 상태에서 강화할 골드가 없을 때만 사용 가능.
/// </summary>
public class EmergencyFundCommand : Command
{
    /// <summary>나무검 상태이고, 첫 강화 비용조차 없을 때만 허용.</summary>
    public override string Validate(GameState state, GameContext context)
    {
        if (state.CurrentLevel != 0)
            return "not_wooden_sword";

        // 첫 강화 비용을 감당할 수 있으면 지원금 불필요
        var firstEnhanceSword = context.SwordTable.GetSword(1);
        if (firstEnhanceSword != null && state.PlayerData.Gold >= firstEnhanceSword.EnhanceCost)
            return "has_enough_gold";

        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var result = context.EconomyLogic.AddGold(state, GameConstants.EmergencyFundAmount, "emergency_fund");
        return new CommandResult(result.NewState, result.Events);
    }
}
