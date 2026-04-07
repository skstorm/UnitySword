using CuiApp.Rendering;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public class EnhanceScreen : IScreen
{
    private readonly ConsoleRenderer _r;

    public EnhanceScreen(ConsoleRenderer renderer)
    {
        _r = renderer;
    }

    public void Render(GameState state, GameContext context)
    {
        var sword = state.CurrentSword;
        var level = state.CurrentLevel;
        var gold = state.PlayerData.Gold;
        var table = context.SwordTable;

        // Header
        Console.WriteLine();
        _r.PrintLine($"  {_r.FormatGold(gold)}");
        _r.PrintLine();

        // Sword ASCII art
        var art = SwordAsciiArt.Get(level);
        foreach (var line in art)
            _r.PrintCenter(line);

        _r.PrintLine();
        _r.PrintCenter($"{sword.Name} +{level}", ConsoleColor.White);
        _r.PrintLine();

        // Action menu
        _r.PrintLine("  ─────────────────────────────");

        if (level < table.MaxLevel)
        {
            var nextSword = table.GetSword(level + 1);
            if (nextSword != null)
            {
                var rateText = level >= 14 ? "???" : $"{nextSword.SuccessRate * 100:F0}%";
                var costText = $"{nextSword.EnhanceCost:N0}G";
                var canAfford = gold >= nextSword.EnhanceCost;
                var affordColor = canAfford ? ConsoleColor.White : ConsoleColor.DarkRed;
                _r.Print("  [E] 강화 (");
                _r.Print(rateText, ConsoleColor.Cyan);
                _r.Print(" / ");
                _r.Print(costText, affordColor);
                _r.PrintLine(")");
            }
        }

        if (level > 0)
        {
            _r.PrintLine($"  [S] 판매 ({sword.SellPrice:N0}G)");
        }

        if (level >= 10 && sword.Collectable)
        {
            _r.PrintLine("  [C] 수집");
        }

        // Emergency fund condition: wooden sword + gold < enhance cost of +1
        var firstSword = table.GetSword(1);
        if (level == 0 && firstSword != null && gold < firstSword.EnhanceCost)
        {
            _r.PrintLine("  [F] 긴급 지원금 (200G)", ConsoleColor.Yellow);
        }

        _r.PrintLine($"  [W] 공방    [Q] 종료");
    }

    public string? HandleInput(char key, GameEngine engine, GameContext context)
    {
        switch (char.ToLower(key))
        {
            case 'e':
                engine.Dispatch(new EnhanceCommand());
                return null;
            case 's':
                engine.Dispatch(new SellCommand());
                return null;
            case 'c':
                engine.Dispatch(new CollectCommand());
                return null;
            case 'f':
                engine.Dispatch(new EmergencyFundCommand());
                return null;
            case 'w':
                return "workshop";
            default:
                return null;
        }
    }
}
