using GameCore.Data;
using GameCore.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TerminalUI
{
    /// <summary>
    /// 중앙 게임 영역. 검 ASCII 아트 + 강화 정보 + 최근 이벤트를 표시한다.
    /// </summary>
    public class SwordView
    {
        private readonly SwordDataTable _swordTable;
        private string _lastEvent;

        public SwordView(SwordDataTable swordTable)
        {
            _swordTable = swordTable;
        }

        /// <summary>최근 이벤트 마크업을 설정한다.</summary>
        public void SetLastEvent(string eventMarkup)
        {
            _lastEvent = eventMarkup;
        }

        public IRenderable Render(GameState state)
        {
            var level = state.CurrentLevel;
            var sword = state.CurrentSword;
            var color = SwordArt.GetColor(level);
            var artLines = SwordArt.GetArt(level);

            // 검 아트를 색상 마크업으로 감싸기
            var artText = string.Join("\n",
                System.Array.ConvertAll(artLines, line =>
                    $"[{color}]{Escape(line)}[/]"));

            // 오른쪽 강화 정보
            var levelStr = level > 0 ? $"+{level} " : "";
            var info = $"[bold {color}]{levelStr}{Escape(sword.Name)}[/]\n";
            info += $"[grey]{Escape(sword.Theme)}[/]\n\n";

            if (level < _swordTable.MaxLevel)
            {
                var next = _swordTable.GetSword(level + 1);
                // +15 이상은 성공률을 ???로 숨김
                var rateStr = level + 1 <= 14 ? $"{next.SuccessRate * 100:F1}%" : "[red]???[/]";
                info += $"강화 확률: {rateStr}\n";
                info += $"강화 비용: [yellow]{next.EnhanceCost:N0}G[/]\n";
            }
            else
            {
                info += "[bold yellow]*** 최고 단계 달성! ***[/]\n";
            }

            if (level > 0)
                info += $"판매가: [cyan]{sword.SellPrice:N0}G[/]\n";

            info += $"보호: {(state.HasActiveProtection ? "[blue]✓ 활성[/]" : "[grey]✗[/]")}";

            // 2열 레이아웃: 검 아트 | 정보
            var columns = new Columns(
                new Markup(artText),
                new Markup(info)
            );

            // 최근 이벤트
            var eventLine = _lastEvent != null
                ? new Markup($"\n  {_lastEvent}")
                : new Markup("");

            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow(columns);
            grid.AddRow(eventLine);

            return new Panel(grid)
                .Header($"[bold {color}]⚔ 대장간[/]")
                .BorderColor(Color.Grey)
                .Expand();
        }

        private static string Escape(string text)
        {
            return text.Replace("[", "[[").Replace("]", "]]");
        }
    }
}
