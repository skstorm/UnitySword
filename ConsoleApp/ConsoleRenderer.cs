using System;
using GameCore.Data;
using GameCore.Events;
using GameCore.Models;

namespace ConsoleApp
{
    public class ConsoleRenderer
    {
        private readonly SwordDataTable _swordTable;

        public ConsoleRenderer(SwordDataTable swordTable)
        {
            _swordTable = swordTable;
        }

        public void PrintStatus(GameState state)
        {
            var sword = state.CurrentSword;
            var level = state.CurrentLevel;
            var gold = state.PlayerData.Gold;
            var stats = state.PlayerData.Stats;
            var fragments = state.PlayerData.Fragments;
            var mastery = state.PlayerData.Mastery;
            var inventory = state.PlayerData.Inventory;
            var collection = state.PlayerData.Collection;

            Console.WriteLine("─────────────────────────────────");
            var levelStr = level > 0 ? $"+{level} " : "";
            Console.WriteLine($"  검: {levelStr}{sword.Name} ({sword.Theme})");
            Console.WriteLine($"  골드: {gold:N0}G");
            Console.WriteLine($"  파편: {fragments}개");
            Console.WriteLine($"  마스터리: Lv.{mastery.Level}");
            Console.WriteLine($"  보호부적: {inventory.ProtectionAmulets}개 | 보호 활성: {(state.HasActiveProtection ? "✓" : "✗")}");
            Console.WriteLine($"  도감: {collection.UniqueCount}/{collection.TotalCollectible} ({collection.CompletionRate * 100:F0}%)");

            if (level < _swordTable.MaxLevel)
            {
                var next = _swordTable.GetSword(level + 1);
                var rateStr = level + 1 <= 14 ? $"{next.SuccessRate * 100:F1}%" : "???";
                Console.WriteLine($"  다음 강화: {rateStr} / {next.EnhanceCost:N0}G");
            }
            else
            {
                Console.WriteLine("  *** 최고 단계 달성! ***");
            }

            if (level > 0)
            {
                Console.WriteLine($"  판매가: {sword.SellPrice:N0}G (회수율 {sword.ReturnRate * 100:F0}%)");
            }

            Console.WriteLine($"  통계: 최고+{stats.HighestEnhanceLevel} | 파괴 {stats.TotalDestroys}회 | 시도 {stats.TotalEnhanceAttempts}회");

            if (stats.CurrentConsecutiveSuccess > 1)
                Console.WriteLine($"  🔥 연속 성공 {stats.CurrentConsecutiveSuccess}회!");
            if (stats.CurrentConsecutiveFail > 1)
                Console.WriteLine($"  💀 연속 파괴 {stats.CurrentConsecutiveFail}회...");
        }

        public void HandleEvent(GameEvent evt)
        {
            switch (evt)
            {
                case EnhanceSuccessEvent e:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ★ 강화 성공! +{e.PrevLevel} → +{e.NewLevel} {e.NewSwordName}");
                    Console.ResetColor();
                    break;

                case EnhanceFailEvent e when e.Destroyed:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✕ 파괴! +{e.DestroyedLevel} {e.DestroyedSwordName}");
                    if (e.DestroyedLevel > 0)
                    {
                        var nextSword = _swordTable.GetSword(e.DestroyedLevel + 1);
                        if (nextSword != null)
                            Console.WriteLine($"    성공했다면 {nextSword.SellPrice:N0}G 였는데...");
                    }
                    Console.ResetColor();
                    break;

                case EnhanceFailEvent e when !e.Destroyed:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ◆ 보호! 강화 실패했지만 파괴를 막았습니다.");
                    Console.ResetColor();
                    break;

                case SellEvent e:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  $ 판매! +{e.SoldLevel} {e.SoldSwordName} → {e.GoldGained:N0}G 획득");
                    Console.ResetColor();
                    break;

                case DestroyConfirmedEvent e:
                    Console.WriteLine($"  나무검으로 돌아갑니다...");
                    break;

                case FragmentGainEvent e:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  ◆ 파편 +{e.TotalGained} 획득 (보유: {e.NewTotal})");
                    Console.ResetColor();
                    break;

                case ExchangeEvent e:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    var itemName = e.ItemType switch
                    {
                        ItemTypes.ProtectionAmulet => "보호부적",
                        ItemTypes.GoldPouch => "골드주머니",
                        _ => e.ItemType
                    };
                    Console.WriteLine($"  🛒 {itemName} 구매 (파편 -{e.FragmentCost})");
                    Console.ResetColor();
                    break;

                case UseItemEvent e:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"  🛡 보호부적 활성화!");
                    Console.ResetColor();
                    break;

                case CollectEvent e:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  📖 +{e.SwordLevel} {e.SwordName} 도감에 등록! ({e.UniqueCount}/{e.TotalCollectible})");
                    Console.ResetColor();
                    break;

                case CollectionCompleteEvent:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  🏆 도감 완성!");
                    Console.ResetColor();
                    break;

                case MasteryExpEvent e:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"  ⚡ 마스터리 경험치 +1 (Lv.{e.CurrentLevel})");
                    Console.ResetColor();
                    break;

                case MasteryLevelUpEvent e:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  ⚡ 마스터리 Lv.{e.NewLevel} 달성! {e.RewardDescription}");
                    Console.ResetColor();
                    break;

                case CommandRejectedEvent e:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    var reason = e.Reason switch
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
                        _ => e.Reason
                    };
                    Console.WriteLine($"  [{reason}]");
                    Console.ResetColor();
                    break;

                case GoldChangeEvent:
                    // Handled by status display
                    break;
            }
        }
    }
}
