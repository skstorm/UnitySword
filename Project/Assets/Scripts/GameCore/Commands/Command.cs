using System.Collections.Generic;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Commands;

/// <summary>
/// 모든 게임 커맨드의 기본 클래스.
/// Validate → Execute 순서로 호출되며, GameEngine이 이 흐름을 관리한다.
/// </summary>
public abstract class Command
{
    public int Timestamp { get; set; }

    /// <summary>실행 전 조건 검증. 실패 시 사유 문자열, 성공 시 null 반환.</summary>
    public abstract string Validate(GameState state, GameContext context);

    /// <summary>커맨드 실행. 새로운 GameState와 발생한 이벤트 목록을 반환.</summary>
    public abstract CommandResult Execute(GameState state, GameContext context);
}

/// <summary>커맨드 실행 결과. 불변 상태 + 이벤트 리스트를 담는 컨테이너.</summary>
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
