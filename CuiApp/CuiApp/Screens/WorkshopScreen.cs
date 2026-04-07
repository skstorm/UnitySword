using CuiApp.Rendering;
using GameCore.Commands;
using GameCore.Data;
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

        // --- Section 1: Mastery ---
        RenderMastery(pd);

        // --- Section 2: Fragment Exchange ---
        RenderExchange(pd);

        // --- Section 3: Collection ---
        RenderCollection(pd, context);

        // --- Section 4: Statistics ---
        RenderStats(pd);

        _r.PrintLine("  ─────────────────────────────");
        _r.PrintLine("  [B] 돌아가기");
    }

    private void RenderMastery(PlayerData pd)
    {
        var masteryLevel = MasteryTable.GetLevel(pd.MasteryExp);
        var title = MasteryTable.GetTitle(masteryLevel);
        var (currentExp, nextExp) = MasteryTable.GetExpRange(pd.MasteryExp);

        _r.Print("  [숙련도] ", ConsoleColor.Yellow);
        _r.PrintLine($"Lv.{masteryLevel} {title}");

        if (nextExp > currentExp)
        {
            var progress = pd.MasteryExp - currentExp;
            var needed = nextExp - currentExp;
            var bar = _r.ProgressBar(progress, needed, 20);
            _r.PrintLine($"  {bar} {pd.MasteryExp}/{nextExp} EXP");
        }
        else
        {
            _r.PrintLine($"  MAX — {pd.MasteryExp} EXP", ConsoleColor.Magenta);
        }
        _r.PrintLine();
    }

    private void RenderExchange(PlayerData pd)
    {
        _r.Print("  [파편 교환소] ", ConsoleColor.Green);
        _r.PrintLine($"보유 파편: {pd.Fragments}개");
        _r.PrintLine();

        var amuletCost = 10;
        var scrollCost = 15;
        var pouchCost = 5;

        var canAmulet = pd.Fragments >= amuletCost;
        var canScroll = pd.Fragments >= scrollCost;
        var canPouch = pd.Fragments >= pouchCost;

        _r.Print("  [1] 보호 부적 ");
        _r.Print($"({amuletCost}파편)", canAmulet ? ConsoleColor.White : ConsoleColor.DarkGray);
        _r.PrintLine($" — 파괴 방지  보유:{pd.Items.ProtectionAmulets}개");

        _r.Print("  [2] 축복 주문서 ");
        _r.Print($"({scrollCost}파편)", canScroll ? ConsoleColor.White : ConsoleColor.DarkGray);
        _r.PrintLine($" — 성공률+5%  보유:{pd.Items.BlessingScrolls}개");

        _r.Print("  [3] 골드 주머니 ");
        _r.Print($"({pouchCost}파편)", canPouch ? ConsoleColor.White : ConsoleColor.DarkGray);
        _r.PrintLine($" — +500G  보유:{pd.Items.GoldPouches}개");

        _r.PrintLine();
    }

    private void RenderCollection(PlayerData pd, GameContext context)
    {
        _r.Print("  [컬렉션 도감] ", ConsoleColor.Magenta);

        // Collectable swords: levels 10~20
        var collectableLevels = new List<int>();
        for (int i = 10; i <= context.SwordTable.MaxLevel; i++)
        {
            var sword = context.SwordTable.GetSword(i);
            if (sword != null && sword.Collectable)
                collectableLevels.Add(i);
        }

        var collected = pd.CollectedSwords;
        var collectedCount = 0;
        foreach (var lvl in collectableLevels)
        {
            if (collected.Contains(lvl)) collectedCount++;
        }

        _r.PrintLine($"{collectedCount}/{collectableLevels.Count}");
        _r.PrintLine();

        foreach (var lvl in collectableLevels)
        {
            var sword = context.SwordTable.GetSword(lvl);
            if (sword == null) continue;

            var isCollected = collected.Contains(lvl);
            var mark = isCollected ? "✦" : "□";
            var color = isCollected ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
            var name = isCollected ? $"+{lvl} {sword.Name}" : $"+{lvl} ???";

            _r.Print($"  {mark} ", color);
            _r.PrintLine(name, color);
        }
        _r.PrintLine();
    }

    private void RenderStats(PlayerData pd)
    {
        _r.PrintLine("  [통계]", ConsoleColor.Cyan);
        _r.PrintLine($"  역대 최고 강화: +{pd.Stats.HighestEnhance}");
        _r.PrintLine($"  총 강화 시도: {pd.Stats.TotalEnhances}회");
        _r.PrintLine($"  총 파괴: {pd.Stats.TotalDestroys}회");
        _r.PrintLine($"  연속 성공 최대: {pd.Stats.MaxConsecutiveSuccess}회");
        _r.PrintLine($"  연속 파괴 최대: {pd.Stats.MaxConsecutiveFail}회");
        _r.PrintLine();
    }

    public string? HandleInput(char key, GameEngine engine, GameContext context)
    {
        switch (key)
        {
            case '1':
                engine.Dispatch(new ExchangeCommand(ExchangeItemType.ProtectionAmulet));
                return null;
            case '2':
                engine.Dispatch(new ExchangeCommand(ExchangeItemType.BlessingScroll));
                return null;
            case '3':
                engine.Dispatch(new ExchangeCommand(ExchangeItemType.GoldPouch));
                return null;
            case 'b':
            case 'B':
                return "enhance";
            default:
                return null;
        }
    }
}
