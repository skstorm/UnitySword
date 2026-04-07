using System;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Models;

namespace GameCore.Engine;

public class GameEngine
{
    private GameState _state;
    private readonly GameContext _context;

    public event Action<GameEvent> OnEvent;

    public GameState State => _state;

    public GameEngine(GameState initialState, GameContext context)
    {
        _state = initialState;
        _context = context;
    }

    public void Dispatch(Command command)
    {
        var rejection = command.Validate(_state, _context);
        if (rejection != null)
        {
            OnEvent?.Invoke(new CommandRejectedEvent(
                command.GetType().Name,
                rejection,
                _context.Time.Now()));
            return;
        }

        var result = command.Execute(_state, _context);
        _state = result.NewState;

        foreach (var evt in result.Events)
        {
            OnEvent?.Invoke(evt);
        }
    }
}
