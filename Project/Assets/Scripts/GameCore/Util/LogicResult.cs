using System.Collections.Generic;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Util;

public class LogicResult
{
    public GameState NewState { get; }
    public List<GameEvent> Events { get; }

    public LogicResult(GameState newState, List<GameEvent> events = null)
    {
        NewState = newState;
        Events = events ?? new List<GameEvent>();
    }
}
