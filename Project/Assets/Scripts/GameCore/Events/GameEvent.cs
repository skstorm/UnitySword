namespace GameCore.Events
{
    public abstract class GameEvent
    {
    }

    public class CommandRejectedEvent : GameEvent
    {
        public string CommandType { get; }
        public string Reason { get; }

        public CommandRejectedEvent(string commandType, string reason)
        {
            CommandType = commandType;
            Reason = reason;
        }
    }
}
