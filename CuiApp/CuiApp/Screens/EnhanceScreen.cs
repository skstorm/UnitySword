using CuiApp.Rendering;
using GameCore.Commands;
using GameCore.Engine;
using GameCore.Models;

namespace CuiApp.Screens;

public class EnhanceScreen : IScreen
{
    private readonly ConsoleRenderer _r;
    private bool _showItemMenu;

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

        // Active effects
        if (state.HasActiveProtection)
            _r.PrintCenter("🛡 보호 활성화", ConsoleColor.Cyan);
        if (state.ActiveModifiers.Count > 0)
            _r.PrintCenter("✨ 축복 활성화", ConsoleColor.Magenta);

        _r.PrintLine();

        if (_showItemMenu)
        {
            RenderItemMenu(state);
        }
        else
        {
            RenderMainMenu(state, context);
        }
    }

    private void RenderMainMenu(GameState state, GameContext context)
    {
        var level = state.CurrentLevel;
        var gold = state.PlayerData.Gold;
        var table = context.SwordTable;
        var items = state.PlayerData.Items;

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
            _r.PrintLine($"  [S] 판매 ({state.CurrentSword.SellPrice:N0}G)");
        }

        if (level >= 10 && state.CurrentSword.Collectable)
        {
            var alreadyCollected = state.PlayerData.CollectedSwords.Contains(level);
            if (alreadyCollected)
                _r.PrintLine("  [C] 수집 (수집 완료)", ConsoleColor.DarkGray);
            else
                _r.PrintLine("  [C] 수집", ConsoleColor.Magenta);
        }

        // Emergency fund
        var firstSword = table.GetSword(1);
        if (level == 0 && firstSword != null && gold < firstSword.EnhanceCost)
        {
            _r.PrintLine("  [F] 긴급 지원금 (200G)", ConsoleColor.Yellow);
        }

        // Item menu (show if has any items)
        var hasItems = items.ProtectionAmulets > 0 || items.BlessingScrolls > 0 || items.GoldPouches > 0;
        if (hasItems)
        {
            _r.PrintLine("  [I] 아이템 사용", ConsoleColor.Green);
        }

        _r.PrintLine($"  [W] 공방    [Q] 종료");
    }

    private void RenderItemMenu(GameState state)
    {
        var items = state.PlayerData.Items;

        _r.PrintLine("  ─── 아이템 ─────────────────");
        _r.PrintLine();

        if (items.ProtectionAmulets > 0)
            _r.PrintLine($"  [1] 보호 부적 x{items.ProtectionAmulets} — 파괴 방지");
        else
            _r.PrintLine("  [1] 보호 부적 x0", ConsoleColor.DarkGray);

        if (items.BlessingScrolls > 0)
            _r.PrintLine($"  [2] 축복 주문서 x{items.BlessingScrolls} — 성공률 +5%");
        else
            _r.PrintLine("  [2] 축복 주문서 x0", ConsoleColor.DarkGray);

        if (items.GoldPouches > 0)
            _r.PrintLine($"  [3] 골드 주머니 x{items.GoldPouches} — +500G");
        else
            _r.PrintLine("  [3] 골드 주머니 x0", ConsoleColor.DarkGray);

        _r.PrintLine();
        _r.PrintLine("  [I] 돌아가기");
    }

    public string? HandleInput(char key, GameEngine engine, GameContext context)
    {
        if (_showItemMenu)
        {
            return HandleItemInput(key, engine);
        }

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
            case 'i':
                _showItemMenu = true;
                return null;
            case 'w':
                return "workshop";
            default:
                return null;
        }
    }

    private string? HandleItemInput(char key, GameEngine engine)
    {
        switch (key)
        {
            case '1':
                engine.Dispatch(new UseItemCommand(UseItemType.ProtectionAmulet));
                _showItemMenu = false;
                return null;
            case '2':
                engine.Dispatch(new UseItemCommand(UseItemType.BlessingScroll));
                _showItemMenu = false;
                return null;
            case '3':
                engine.Dispatch(new UseItemCommand(UseItemType.GoldPouch));
                _showItemMenu = false;
                return null;
            case 'i':
            case 'I':
                _showItemMenu = false;
                return null;
            default:
                return null;
        }
    }
}
