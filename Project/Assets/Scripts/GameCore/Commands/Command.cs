using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Commands;

public abstract class Command
{
    public int Timestamp { get; set; }

    public abstract string Validate(GameState state, GameContext context);
    public abstract CommandResult Execute(GameState state, GameContext context);
}

public class CommandResult
{
    public GameState NewState { get; }
    public List<GameEvent> Events { get; }

    public CommandResult(GameState newState, List<GameEvent> events = null)
    {
        NewState = newState;
        Events = events ?? new List<GameEvent>();
    }
}
