using GameCore.Events;

namespace CuiApp.Rendering;

public class EventRenderer
{
    private readonly Queue<GameEvent> _queue = new();
    private readonly ConsoleRenderer _r;
    public bool ShouldSave { get; private set; }

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
                _r.Flash(ConsoleColor.Green);
                Console.WriteLine();
                _r.PrintCenter($"★ 강화 성공! +{e.NewLevel} {e.SwordName} ★", ConsoleColor.Green);
                _r.Pause(e.NewLevel >= 10 ? 1200 : 800);
                break;

            case EnhanceFailEvent e when e.Destroyed:
                _r.Flash(ConsoleColor.Red);
                Console.WriteLine();
                _r.PrintCenter($"✖ {e.SwordName} +{e.Level} 파괴!", ConsoleColor.Red);
                if (e.SellPrice > 0)
                    _r.PrintCenter($"\"{e.SellPrice:N0}G짜리 검이 가루가 됐다...\"", ConsoleColor.DarkRed);
                _r.WaitKey();
                ShouldSave = true;
                break;

            case EnhanceFailEvent e when !e.Destroyed:
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
                _r.PrintCenter($"수집 완료! {e.SwordName} → 컬렉션 등록", ConsoleColor.Magenta);
                _r.Pause(800);
                ShouldSave = true;
                break;

            case CommandRejectedEvent e:
                Console.WriteLine();
                _r.PrintCenter($"[!] {GetRejectMessage(e.Reason)}", ConsoleColor.DarkRed);
                _r.Pause(500);
                break;

            case GoldChangeEvent:
                // Suppress — gold changes are shown as part of other events
                break;
        }
    }

    private static string GetRejectMessage(string reason) => reason switch
    {
        "insufficient_gold" => "골드가 부족합니다",
        "cannot_sell_wooden_sword" => "나무검은 판매할 수 없습니다",
        "level_too_low" => "+10 이상만 수집 가능합니다",
        "not_wooden_sword" => "나무검 상태에서만 지원금을 받을 수 있습니다",
        "has_enough_gold" => "강화할 골드가 충분합니다",
        "pending_ad_protection" => "진행 중인 작업이 있습니다",
        _ => reason
    };
}
