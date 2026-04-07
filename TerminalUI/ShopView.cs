using GameCore.Commands;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 상점 화면. 파편으로 교환 가능한 아이템 목록을 표시한다.
    /// 비용은 ExchangeCommand 상수를 참조하여 표시/로직 간 불일치를 방지한다.
    /// </summary>
    public static class ShopView
    {
        public static IRenderable Render(GameState state)
        {
            var grid = new Grid();
            grid.AddColumn();

            grid.AddRow(new Markup(
                $"  [blue]🛡 보호부적[/]         " +
                $"[cyan]{ExchangeCommand.ProtectionAmuletCost} 파편[/]\n" +
                $"  [grey]   강화 실패 시 파괴를 1회 방지[/]"));

            grid.AddEmptyRow();

            grid.AddRow(new Markup(
                $"  [yellow]💰 골드주머니[/]       " +
                $"[cyan]{ExchangeCommand.GoldPouchCost} 파편[/] → " +
                $"[yellow]{ExchangeCommand.GoldPouchReward}G[/]\n" +
                $"  [grey]   즉시 골드 획득[/]"));

            return new Panel(grid)
                .Header("[bold fuchsia]🏪 상점[/]")
                .BorderColor(Color.Fuchsia)
                .Expand();
        }
    }
}
