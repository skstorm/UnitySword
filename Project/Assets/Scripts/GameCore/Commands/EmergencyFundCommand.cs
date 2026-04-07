using GameCore.Engine;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands;

public class EmergencyFundCommand : Command
{
    public override string Validate(GameState state, GameContext context)
    {
        if (state.CurrentLevel != 0)
            return "not_wooden_sword";

        var firstEnhanceSword = context.SwordTable.GetSword(1);
        if (firstEnhanceSword != null && state.PlayerData.Gold >= firstEnhanceSword.EnhanceCost)
            return "has_enough_gold";

        return null;
    }

    public override CommandResult Execute(GameState state, GameContext context)
    {
        var economyLogic = new EconomyLogic();
        var result = economyLogic.AddGold(state, 200, "emergency_fund");
        return new CommandResult(result.NewState, result.Events);
    }
}
