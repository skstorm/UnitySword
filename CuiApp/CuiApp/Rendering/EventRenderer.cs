using GameCore.Events;

namespace CuiApp.Rendering;

/// <summary>
/// 게임 이벤트를 큐에 모았다가 순차적으로 콘솔에 렌더링한다.
/// 서스펜스 연출, 도발 메시지, 세션 통계 추적도 담당.
/// </summary>
public class EventRenderer
{
    private readonly Queue<GameEvent> _queue = new();
    private readonly ConsoleRenderer _r;
    private readonly Random _random = new();

    /// <summary>상태 변경이 있었으므로 저장이 필요한지 여부.</summary>
    public bool ShouldSave { get; private set; }

    // ── 세션 통계 (종료 시 리포트 표시용) ──
    public int SessionEnhances { get; private set; }
    public int SessionDestroys { get; private set; }
    public int SessionGoldDelta { get; private set; }
    public int SessionHighestLevel { get; private set; }

    public EventRenderer(ConsoleRenderer renderer)
    {
        _r = renderer;
    }

    public void Enqueue(GameEvent evt) => _queue.Enqueue(evt);

    public void PlayAll()
    {
        ShouldSave = false;
        while (_queue.Count > 0)
        {
            var evt = _queue.Dequeue();
            Render(evt);
        }
    }

    private void Render(GameEvent evt)
    {
        switch (evt)
        {
            case EnhanceSuccessEvent e:
                SessionEnhances++;
                if (e.NewLevel > SessionHighestLevel)
                    SessionHighestLevel = e.NewLevel;
                PlaySuspense(e.PreviousLevel);
                _r.Flash(ConsoleColor.Green);
                Console.WriteLine();
                _r.PrintCenter($"★ 강화 성공! +{e.NewLevel} {e.SwordName} ★", ConsoleColor.Green);
                _r.Pause(e.NewLevel >= 10 ? 1200 : 800);
                break;

            case EnhanceFailEvent e when e.Destroyed:
                SessionEnhances++;
                SessionDestroys++;
                PlaySuspense(e.Level);
                _r.Flash(ConsoleColor.Red);
                Console.WriteLine();
                _r.PrintCenter($"✖ {e.SwordName} +{e.Level} 파괴!", ConsoleColor.Red);
                _r.PrintCenter(GetTauntMessage(e.Level, e.SellPrice), ConsoleColor.DarkRed);
                if (e.FragmentReward > 0)
                    _r.PrintCenter($"파편 +{e.FragmentReward}개 획득", ConsoleColor.DarkYellow);
                _r.WaitKey();
                ShouldSave = true;
                break;

            case EnhanceFailEvent e when !e.Destroyed:
                SessionEnhances++;
                _r.Flash(ConsoleColor.Cyan);
                Console.WriteLine();
                _r.PrintCenter("부적이 빛나며 검을 보호했다!", ConsoleColor.Cyan);
                _r.Pause(800);
                break;

            case SellEvent e:
                _r.Flash(ConsoleColor.Yellow);
                Console.WriteLine();
                _r.PrintCenter($"판매 완료! +{e.Gold:N0}G", ConsoleColor.Yellow);
                _r.Pause(600);
                ShouldSave = true;
                break;

            case GoldChangeEvent e when e.Reason == "emergency_fund":
                Console.WriteLine();
                _r.PrintCenter($"긴급 지원금 +{e.Amount:N0}G!", ConsoleColor.Yellow);
                _r.Pause(500);
                break;

            case CollectEvent e:
                _r.Flash(ConsoleColor.Magenta);
                Console.WriteLine();
                _r.PrintCenter($"✦ 수집 완료! {e.SwordName} → 컬렉션 등록 ✦", ConsoleColor.Magenta);
                _r.Pause(800);
                ShouldSave = true;
                break;

            case ExchangeEvent e:
                _r.Flash(ConsoleColor.Green);
                Console.WriteLine();
                _r.PrintCenter($"교환 완료! {e.ItemName} 획득 (-{e.FragmentCost} 파편)", ConsoleColor.Green);
                _r.Pause(600);
                ShouldSave = true;
                break;

            case ItemUsedEvent e:
                _r.Flash(ConsoleColor.Cyan);
                Console.WriteLine();
                _r.PrintCenter($"♦ {e.ItemName} 사용! {e.Effect}", ConsoleColor.Cyan);
                _r.Pause(600);
                break;

            case CommandRejectedEvent e:
                Console.WriteLine();
                _r.PrintCenter($"[!] {GetRejectMessage(e.Reason)}", ConsoleColor.DarkRed);
                _r.Pause(500);
                break;

            case GoldChangeEvent e:
                SessionGoldDelta += e.Amount;
                break;
        }
    }

    private void PlaySuspense(int level)
    {
        var delay = level switch
        {
            >= 18 => 3000,
            >= 15 => 2000,
            >= 10 => 1200,
            >= 6 => 600,
            _ => 0
        };

        if (delay <= 0) return;

        Console.WriteLine();
        _r.PrintCenter("강화 중 . . .", ConsoleColor.DarkYellow);

        // Animated dots for high-level suspense
        if (delay >= 1200)
        {
            var steps = delay >= 2000 ? 4 : 3;
            var stepDelay = delay / steps;
            for (int i = 0; i < steps; i++)
            {
                _r.Pause(stepDelay);
                var dots = new string('.', i + 1);
                _r.PrintCenter(dots, ConsoleColor.DarkGray);
            }
        }
        else
        {
            _r.Pause(delay);
        }
    }

    private string GetTauntMessage(int level, int sellPrice)
    {
        string[] lowTaunts =
        [
            "다음엔 되겠지...아마?",
            "흔한 일이야, 흔한 일.",
            "검이 부서지는 소리가 경쾌하다.",
            "나무검이 기다리고 있어!",
            "강화는 원래 이런 거야.",
        ];

        string[] midTaunts =
        [
            $"\"{sellPrice:N0}G짜리 검이 가루가 됐다...\"",
            "장인의 손이 미끄러졌다!",
            "검이 비명을 지르며 산산조각났다!",
            $"+{level} 검의 마지막 빛이 사라졌다...",
            "운명의 여신이 등을 돌렸다.",
            "\"그 돈이면 치킨을...\"",
        ];

        string[] highTaunts =
        [
            $"+{level}... 전설이 될 뻔했는데.",
            $"\"{sellPrice:N0}G의 꿈이 사라졌다...\"",
            "우주가 당신의 강화를 거부했다!",
            "신화급 검이 먼지가 되었다...",
            "강화석이 눈물을 흘린다.",
            "\"다시는 이 레벨을 볼 수 없을지도...\"",
            "전설은 쉽게 탄생하지 않는 법.",
        ];

        var pool = level switch
        {
            >= 15 => highTaunts,
            >= 8 => midTaunts,
            _ => lowTaunts,
        };

        return pool[_random.Next(pool.Length)];
    }

    private static string GetRejectMessage(string reason) => reason switch
    {
        "insufficient_gold" => "골드가 부족합니다",
        "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다",
        "level_too_low" => "+10 이상만 수집 가능합니다",
        "not_wooden_sword" => "나무검 상태에서만 지원금을 받을 수 있습니다",
        "has_enough_gold" => "강화할 골드가 충분합니다",
        "pending_ad_protection" => "진행 중인 작업이 있습니다",
        "insufficient_fragments" => "파편이 부족합니다",
        "already_collected" => "이미 수집한 검입니다",
        "not_collectable" => "수집할 수 없는 검입니다",
        "no_protection_amulets" => "보호 부적이 없습니다",
        "no_blessing_scrolls" => "축복 주문서가 없습니다",
        "no_gold_pouches" => "골드 주머니가 없습니다",
        "already_protected" => "이미 보호가 활성화되어 있습니다",
        _ => reason
    };
}
