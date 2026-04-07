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

        var pd = state.PlayerData;
        var hasSave = !pd.IsFirstRun;

        if (hasSave)
        {
            var sword = state.CurrentSword;
            _r.DrawBox([
                "",
                "        ⚔  검 강 화  ⚔        ",
                "",
                $"   ▸ {sword.Name} +{state.CurrentLevel}",
                $"   ▸ {pd.Gold:N0}G | 파편 {pd.Fragments}개",
                $"   ▸ 최고 +{pd.Stats.HighestEnhance}",
                "",
                "   [ S ] 이어하기             ",
                "   [ Q ] 종료                  ",
                "",
            ]);
        }
        else
        {
            _r.DrawBox([
                "",
                "        ⚔  검 강 화  ⚔        ",
                "",
                "   [ S ] 새 게임               ",
                "   [ Q ] 종료                  ",
                "",
            ]);
        }
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
