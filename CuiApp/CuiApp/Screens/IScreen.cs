using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public interface IScreen
{
    void Render(GameState state, GameContext context);
    /// <summary>
    /// Handle input key. Returns next screen name to switch to, or null to stay.
    /// </summary>
    string? HandleInput(char key, GameEngine engine, GameContext context);
}
