using System;
using GameCore.Commands;
using GameCore.Events;
using GameCore.Logic;
using GameCore.Models;

namespace GameCore.Engine
{
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
            // 1. Validate
            var rejection = command.Validate(_state, _context);
            if (rejection != null)
            {
                OnEvent?.Invoke(new CommandRejectedEvent(
                    command.GetType().Name, rejection));
                return;
            }

            // 2. Execute
            var result = command.Execute(_state, _context);

            // 3. Update state
            _state = result.NewState;

            // 4. Publish events
            foreach (var evt in result.Events)
            {
                OnEvent?.Invoke(evt);
            }
        }

        /// <summary>
        /// Create a new GameEngine with initial state from PlayerData.
        /// </summary>
        public static GameEngine Create(PlayerData playerData, GameContext context)
        {
            var sessionLogic = new GameSessionLogic();
            var initialState = sessionLogic.CreateInitialState(playerData, context.SwordTable);
            return new GameEngine(initialState, context);
        }
    }
}
