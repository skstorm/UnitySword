using System.Collections.Generic;
using System.Linq;
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

            // 1. Calculate enhance cost (with mastery discount)
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
                // Calculate discounted next cost for the event
                var nextLevel = s.CurrentLevel + 2; // after success, next target
                var nextSword = context.SwordTable.GetSword(nextLevel);
                var discountedNextCost = nextSword?.EnhanceCost ?? 0;
                if (context.MasteryTable != null && nextSword != null)
                {
                    var discount = new MasteryLogic().GetCostDiscount(s, context.MasteryTable);
                    discountedNextCost = (int)(discountedNextCost * (1.0 - discount));
                }

                var successResult = enhanceLogic.HandleSuccess(s, targetSword, context.SwordTable, discountedNextCost);
                s = successResult.NewState;
                events.AddRange(successResult.Events);
            }
            else
            {
                // Capture before HandleFail mutates state
                var destroyedLevel = s.CurrentLevel;
                var destroyedName = s.CurrentSword.Name;

                var failResult = enhanceLogic.HandleFail(s, context.SwordTable, context);
                s = failResult.NewState;
                events.AddRange(failResult.Events);

                // Finalize destroy only if actually destroyed and not waiting for ad
                var failEvt = failResult.Events.OfType<EnhanceFailEvent>().FirstOrDefault();
                if (failEvt != null && failEvt.Destroyed && !s.PendingAdProtection)
                {
                    var destroyResult = new DestroyFinalizationLogic().Finalize(
                        s, destroyedLevel, destroyedName,
                        context.SwordTable, context.MasteryTable);
                    s = destroyResult.NewState;
                    events.AddRange(destroyResult.Events);
                }
            }

            // 5. Mastery exp +1
            if (context.MasteryTable != null)
            {
                var masteryResult = new MasteryLogic().AddExp(s, context.MasteryTable);
                s = masteryResult.NewState;
                events.AddRange(masteryResult.Events);
            }

            return new CommandResult(s, events);
        }

        private int GetEnhanceCost(GameState state, GameContext context)
        {
            var targetSword = context.SwordTable.GetSword(state.CurrentLevel + 1);
            if (targetSword == null) return int.MaxValue;
            var baseCost = targetSword.EnhanceCost;

            if (context.MasteryTable != null)
            {
                var discount = new MasteryLogic().GetCostDiscount(state, context.MasteryTable);
                baseCost = (int)(baseCost * (1.0 - discount));
            }

            return baseCost;
        }
    }
}
