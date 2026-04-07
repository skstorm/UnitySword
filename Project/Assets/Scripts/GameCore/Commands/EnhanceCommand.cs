using System.Collections.Generic;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Commands
{
    public class EnhanceCommand : Command
    {
        public override string Validate(GameState state, GameContext context)
        {
            if (state.PendingAdProtection) return "pending_ad_protection";

            var targetSword = context.SwordTable.GetSword(state.CurrentLevel + 1);
            if (targetSword == null) return "max_level_reached";

            var cost = GetEnhanceCost(state, context);
            if (!new EconomyLogic().CanAfford(state, cost)) return "insufficient_gold";

            return null;
        }

        public override CommandResult Execute(GameState state, GameContext context)
        {
            var events = new List<GameEvent>();
            var s = state;

            // 1. Calculate enhance cost
            var cost = GetEnhanceCost(s, context);

            // 2. Spend gold
            var spendResult = new EconomyLogic().SpendGold(s, cost);
            s = spendResult.NewState;
            events.AddRange(spendResult.Events);

            // 3. Roll for success
            var enhanceLogic = new EnhanceLogic();
            var targetSword = context.SwordTable.GetSword(s.CurrentLevel + 1);
            var rate = enhanceLogic.GetEffectiveRate(s, targetSword);
            var success = enhanceLogic.Roll(rate, context.Random);

            // 4. Handle success or failure
            if (success)
            {
                var successResult = enhanceLogic.HandleSuccess(s, targetSword, context.SwordTable);
                s = successResult.NewState;
                events.AddRange(successResult.Events);
            }
            else
            {
                var failResult = enhanceLogic.HandleFail(s, context.SwordTable, context);
                s = failResult.NewState;
                events.AddRange(failResult.Events);

                // P1: no ad protection system, always finalize destroy immediately
                if (!s.PendingAdProtection && !s.HasActiveProtection)
                {
                    var destroyResult = FinalizeDestroy(s, context);
                    s = destroyResult.NewState;
                    events.AddRange(destroyResult.Events);
                }
            }

            // 5. Mastery exp +1 [P2에서 활성화]
            // var masteryResult = new MasteryLogic().AddExp(s, context.MasteryTable);
            // s = masteryResult.NewState; events.AddRange(masteryResult.Events);

            return new CommandResult(s, events);
        }

        private LogicResult FinalizeDestroy(GameState state, GameContext context)
        {
            var events = new List<GameEvent>();
            var s = state;

            // Fragment reward [P2에서 활성화]
            // var fragmentBonus = context.MasteryTable.GetLevel(s.PlayerData.Mastery.Level).FragmentBonus;
            // var fragResult = new FragmentLogic().GiveFragments(s, s.CurrentLevel, fragmentBonus);
            // s = fragResult.NewState; events.AddRange(fragResult.Events);

            s = new GameSessionLogic().ResetToWoodenSword(s, context.SwordTable);
            events.Add(new DestroyConfirmedEvent(state.CurrentLevel, state.CurrentSword.Name));
            return new LogicResult(s, events);
        }

        private int GetEnhanceCost(GameState state, GameContext context)
        {
            var targetSword = context.SwordTable.GetSword(state.CurrentLevel + 1);
            return targetSword?.EnhanceCost ?? 0;
        }
    }
}
