using GameCore.Events;
using GameCore.Models;

namespace TerminalUI
{
    /// <summary>
    /// GameEvent를 Spectre.Console 마크업 문자열로 변환한다.
    /// 각 이벤트 타입별 색상과 아이콘을 적용한다.
    /// </summary>
    public static class EventFormatter
    {
        /// <summary>
        /// GameEvent를 마크업 문자열로 변환한다. 표시할 필요 없는 이벤트는 null 반환.
        /// </summary>
        public static string Format(GameEvent evt)
        {
            return evt switch
            {
                EnhanceSuccessEvent e =>
                    $"[green]★ 강화 성공! +{e.PrevLevel} → +{e.NewLevel} {Escape(e.NewSwordName)}[/]",

                EnhanceFailEvent e when e.Destroyed =>
                    $"[red]✕ 파괴! +{e.DestroyedLevel} {Escape(e.DestroyedSwordName)}[/]",

                EnhanceFailEvent e when !e.Destroyed =>
                    $"[yellow]◆ 보호! 강화 실패했지만 파괴를 막았습니다.[/]",

                SellEvent e =>
                    $"[cyan]$ 판매! +{e.SoldLevel} {Escape(e.SoldSwordName)} → {e.GoldGained:N0}G[/]",

                DestroyConfirmedEvent =>
                    "[grey]나무검으로 돌아갑니다...[/]",

                FragmentGainEvent e =>
                    $"[cyan]◆ 파편 +{e.TotalGained} 획득 (보유: {e.NewTotal})[/]",

                ExchangeEvent e =>
                    $"[fuchsia]🛒 {FormatItemName(e.ItemType)} 구매 (파편 -{e.FragmentCost})[/]",

                UseItemEvent =>
                    "[blue]🛡 보호부적 활성화![/]",

                CollectEvent e =>
                    $"[green]📖 +{e.SwordLevel} {Escape(e.SwordName)} 도감에 등록! ({e.UniqueCount}/{e.TotalCollectible})[/]",

                CollectionCompleteEvent =>
                    "[yellow]🏆 도감 완성![/]",

                MasteryExpEvent e =>
                    $"[olive]⚡ 마스터리 경험치 +1 (Lv.{e.CurrentLevel})[/]",

                MasteryLevelUpEvent e =>
                    $"[yellow]⚡ 마스터리 Lv.{e.NewLevel} 달성! {Escape(e.RewardDescription)}[/]",

                CommandRejectedEvent e =>
                    $"[grey]{FormatRejection(e.Reason)}[/]",

                GoldChangeEvent => null,

                _ => null
            };
        }

        private static string FormatItemName(string itemType)
        {
            return itemType switch
            {
                ItemTypes.ProtectionAmulet => "보호부적",
                ItemTypes.GoldPouch => "골드주머니",
                _ => itemType
            };
        }

        private static string FormatRejection(string reason)
        {
            return reason switch
            {
                "insufficient_gold" => "골드가 부족합니다!",
                "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다.",
                "max_level_reached" => "이미 최고 단계입니다!",
                "pending_ad_protection" => "파괴 처리를 먼저 완료해주세요.",
                "no_pending_destruction" => "파괴 대기 상태가 아닙니다.",
                "insufficient_fragments" => "파편이 부족합니다!",
                "no_amulets" => "보호부적이 없습니다.",
                "already_protected" => "이미 보호가 활성화되어 있습니다.",
                "cannot_protect_wooden_sword" => "나무검은 보호할 수 없습니다.",
                "not_collectible" => "수집할 수 없는 검입니다.",
                "invalid_item_type" => "잘못된 아이템입니다.",
                _ => reason
            };
        }

        /// <summary>Spectre 마크업 특수문자([, ]) 이스케이프</summary>
        private static string Escape(string text)
        {
            return text.Replace("[", "[[").Replace("]", "]]");
        }
    }
}
