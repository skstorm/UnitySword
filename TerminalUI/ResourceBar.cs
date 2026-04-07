using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 상단 영구 리소스 바. 골드/파편/마스터리/도감 진행도를 한 줄로 표시한다.
    /// </summary>
    public static class ResourceBar
    {
        public static IRenderable Render(GameState state)
        {
            var p = state.PlayerData;
            var text = new Markup(
                $"  [yellow]💰 {p.Gold:N0}G[/]  " +
                $"[cyan]◆ {p.Fragments}파편[/]  " +
                $"[olive]⚡ Lv.{p.Mastery.Level}[/]  " +
                $"[green]📖 {p.Collection.UniqueCount}/{p.Collection.TotalCollectible}[/]"
            );

            return new Panel(text)
                .Header("[bold]리소스[/]")
                .BorderColor(Color.Grey)
                .Expand();
        }
    }
}
