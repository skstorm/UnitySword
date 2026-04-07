using System;

namespace GameCore.Events;

public abstract class GameEvent
{
    public DateTime Timestamp { get; }

    protected GameEvent(DateTime timestamp)
    {
        Timestamp = timestamp;
    }
}

public class CommandRejectedEvent : GameEvent
{
    public string CommandType { get; }
    public string Reason { get; }

    public CommandRejectedEvent(string commandType, string reason, DateTime timestamp)
        : base(timestamp)
    {
        CommandType = commandType;
        Reason = reason;
    }
}
