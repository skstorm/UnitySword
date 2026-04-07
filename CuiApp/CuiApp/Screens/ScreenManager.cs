using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public class ScreenManager
{
    private readonly Dictionary<string, IScreen> _screens = new();
    private IScreen? _current;
    private string _currentName = "";

    public void Register(string name, IScreen screen) => _screens[name] = screen;

    public void SwitchTo(string name)
    {
        if (_screens.TryGetValue(name, out var screen))
        {
            _current = screen;
            _currentName = name;
        }
    }

    public string CurrentName => _currentName;

    public void Render(GameState state, GameContext context)
        => _current?.Render(state, context);

    public void HandleInput(char key, GameEngine engine, GameContext context)
    {
        var nextScreen = _current?.HandleInput(key, engine, context);
        if (nextScreen != null)
            SwitchTo(nextScreen);
    }
}
