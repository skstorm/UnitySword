using GameCore.Data;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 하단 커맨드 영역. 현재 상태와 화면 모드에 따라 사용 가능한 키 안내를 표시한다.
    /// </summary>
    public static class CommandBar
    {
        /// <summary>메인 게임 화면의 커맨드를 렌더링한다.</summary>
        public static IRenderable RenderMain(GameState state, SwordDataTable swordTable)
        {
            if (state.PendingAdProtection)
            {
                return new Panel(new Markup(
                    "  [bold red][[3]][/] 파괴 확정    [bold][[Q]][/] 종료"))
                    .BorderColor(Color.Grey)
                    .Expand();
            }

            var parts = "";

            if (state.CurrentLevel < swordTable.MaxLevel)
                parts += "[bold green][[1]][/] 강화  ";

            if (state.CurrentLevel > 0)
                parts += "[bold cyan][[2]][/] 판매  ";

            if (state.CurrentSword.Collectible)
                parts += "[bold green][[3]][/] 수집  ";

            if (state.PlayerData.Inventory.ProtectionAmulets > 0)
                parts += "[bold blue][[4]][/] 아이템  ";

            parts += "[bold fuchsia][[5]][/] 상점  ";
            parts += "[bold][[Q]][/] 종료";

            return new Panel(new Markup($"  {parts}"))
                .BorderColor(Color.Grey)
                .Expand();
        }

        /// <summary>상점 화면의 커맨드를 렌더링한다.</summary>
        public static IRenderable RenderShop()
        {
            return new Panel(new Markup(
                "  [bold blue][[1]][/] 보호부적  " +
                "[bold yellow][[2]][/] 골드주머니  " +
                "[bold][[0]][/] 돌아가기"))
                .BorderColor(Color.Fuchsia)
                .Expand();
        }
    }
}
