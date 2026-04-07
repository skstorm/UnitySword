using CuiApp.Rendering;
using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public class WorkshopScreen : IScreen
{
    private readonly ConsoleRenderer _r;

    public WorkshopScreen(ConsoleRenderer renderer)
    {
        _r = renderer;
    }

    public void Render(GameState state, GameContext context)
    {
        var pd = state.PlayerData;
        Console.WriteLine();
        _r.PrintCenter("=== 공 방 ===", ConsoleColor.Cyan);
        _r.PrintLine();

        // Statistics
        _r.PrintLine("  [통계]");
        _r.PrintLine($"  역대 최고 강화: +{pd.Stats.HighestEnhance}");
        _r.PrintLine($"  총 강화 시도: {pd.Stats.TotalEnhances}회");
        _r.PrintLine($"  총 파괴: {pd.Stats.TotalDestroys}회");
        _r.PrintLine($"  연속 성공 최대: {pd.Stats.MaxConsecutiveSuccess}회");
        _r.PrintLine($"  연속 파괴 최대: {pd.Stats.MaxConsecutiveFail}회");
        _r.PrintLine();

        // Fragments (P2 placeholder)
        _r.PrintLine($"  [파편] {pd.Fragments}개");
        _r.PrintLine();

        _r.PrintLine("  ─────────────────────────────");
        _r.PrintLine("  [B] 돌아가기");
    }

    public string? HandleInput(char key, GameEngine engine, GameContext context)
    {
        return char.ToLower(key) switch
        {
            'b' => "enhance",
            _ => null
        };
    }
}
