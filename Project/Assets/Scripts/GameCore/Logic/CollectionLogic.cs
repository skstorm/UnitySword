using System.Collections.Generic;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Logic
{
    internal class CollectionLogic
    {
        public LogicResult Collect(GameState state)
        {
            var events = new List<GameEvent>();
            var sword = state.CurrentSword;
            var collection = state.PlayerData.Collection;

            var isFirstCollect = !collection.Collected.ContainsKey(sword.Level);
            var newCollection = collection.WithCollected(sword.Level);
            var newState = state.With(playerData: state.PlayerData.With(collection: newCollection));

            events.Add(new CollectEvent(
                sword.Level, sword.Name, isFirstCollect,
                newCollection.UniqueCount, newCollection.TotalCollectible));

            if (newCollection.UniqueCount == newCollection.TotalCollectible && isFirstCollect)
                events.Add(new CollectionCompleteEvent());

            return new LogicResult(newState, events);
        }
    }
}
