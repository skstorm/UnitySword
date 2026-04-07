using System;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Engine;

/// <summary>
/// 게임 엔진. Command를 받아 검증 → 실행 → 이벤트 발행 흐름을 관리한다.
/// 모든 상태 변경은 이 클래스를 통해서만 이루어진다.
/// </summary>
public class GameEngine
{
    private GameState _state;
    private readonly GameContext _context;

    /// <summary>이벤트 구독자에게 게임 이벤트를 전달하는 콜백.</summary>
    public event Action<GameEvent> OnEvent;

    public GameState State => _state;

    public GameEngine(GameState initialState, GameContext context)
    {
        _state = initialState;
        _context = context;
    }

    /// <summary>
    /// 커맨드를 검증하고 실행한다.
    /// 모든 결과 이벤트에 동일한 타임스탬프를 일괄 부여한 뒤 발행한다.
    /// </summary>
    public void Dispatch(Command command)
    {
        var now = _context.Time.Now();

        var rejection = command.Validate(_state, _context);
        if (rejection != null)
        {
            OnEvent?.Invoke(new CommandRejectedEvent(
                command.GetType().Name,
                rejection,
                now));
            return;
        }

        var result = command.Execute(_state, _context);
        _state = result.NewState;

        // 모든 이벤트에 동일한 타임스탬프 주입
        foreach (var evt in result.Events)
        {
            evt.Timestamp = now;
            OnEvent?.Invoke(evt);
        }
    }
}
