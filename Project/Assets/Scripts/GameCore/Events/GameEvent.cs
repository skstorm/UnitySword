using System;

namespace GameCore.Events;

/// <summary>
/// 모든 게임 이벤트의 기본 클래스.
/// Timestamp는 GameEngine.Dispatch()에서 일괄 주입된다.
/// </summary>
public abstract class GameEvent
{
    /// <summary>이벤트 발생 시각. GameEngine이 Dispatch 시점에 설정한다.</summary>
    public DateTime Timestamp { get; internal set; }

    protected GameEvent(DateTime timestamp = default)
    {
        Timestamp = timestamp;
    }
}

/// <summary>커맨드 검증 실패 시 발생하는 이벤트.</summary>
public class CommandRejectedEvent : GameEvent
{
    /// <summary>거부된 커맨드의 타입명.</summary>
    public string CommandType { get; }
    /// <summary>거부 사유 코드 (예: "insufficient_gold").</summary>
    public string Reason { get; }

    public CommandRejectedEvent(string commandType, string reason, DateTime timestamp = default)
        : base(timestamp)
    {
        CommandType = commandType;
        Reason = reason;
    }
}
