using CuiApp.Rendering;
using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public class TitleScreen : IScreen
{
    private readonly ConsoleRenderer _r;

    public TitleScreen(ConsoleRenderer renderer)
    {
        _r = renderer;
    }

    public void Render(GameState state, GameContext context)
    {
        Console.WriteLine();
        Console.WriteLine();
        _r.DrawBox([
            "",
            "        ⚔  검 강 화  ⚔        ",
            "",
            "   [ S ] 게임 시작             ",
            "   [ Q ] 종료                  ",
            "",
        ]);
    }

    public string? HandleInput(char key, GameEngine engine, GameContext context)
    {
        return char.ToLower(key) switch
        {
            's' => "enhance",
            _ => null
        };
    }
}
