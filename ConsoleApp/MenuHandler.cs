using System;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Models;
using GameApp;

namespace ConsoleApp
{
    /// <summary>
    /// 콘솔 전용 입력 처리기. 메인 메뉴와 상점 서브메뉴의 키 입력을 받아
    /// GameEngine에 Command를 디스패치하고, 매 액션 후 자동 저장한다.
    /// 플랫폼별 UI(Unity 등)에서는 이 클래스를 대체하여 자체 입력 처리를 구현한다.
    /// </summary>
    public class MenuHandler
    {
        private const string InvalidInputMessage = "  잘못된 입력입니다.";

        private readonly GameEngine _engine;
        private readonly SwordDataTable _swordTable;
        private readonly SaveManager _saveManager;

        public MenuHandler(GameEngine engine, SwordDataTable swordTable, SaveManager saveManager)
        {
            _engine = engine;
            _swordTable = swordTable;
            _saveManager = saveManager;
        }

        /// <summary>
        /// Command를 디스패치하고 변경된 PlayerData를 즉시 저장한다.
        /// 모든 입력 핸들러에서 이 메서드를 사용하여 저장 누락을 방지한다.
        /// </summary>
        private void DispatchAndSave(Command cmd)
        {
            _engine.Dispatch(cmd);
            _saveManager.Save(_engine.State.PlayerData);
        }

        /// <summary>
        /// 현재 GameState에 따라 사용 가능한 메뉴 항목을 콘솔에 출력한다.
        /// 파괴대기 상태에서는 파괴 확정만 표시, 그 외에는 전체 메뉴를 표시한다.
        /// </summary>
        public void PrintMenu(GameState state)
        {
            Console.WriteLine("─────────────────────────────────");

            if (state.PendingAdProtection)
            {
                Console.WriteLine("  [3] 파괴 확정 (나무검으로 리셋)");
                Console.WriteLine("  [Q] 종료");
            }
            else
            {
                if (state.CurrentLevel < _swordTable.MaxLevel)
                    Console.WriteLine("  [1] 강화");

                if (state.CurrentLevel > 0)
                    Console.WriteLine("  [2] 판매");

                if (state.CurrentSword.Collectible)
                    Console.WriteLine("  [3] 수집 (도감에 등록)");

                if (state.PlayerData.Inventory.ProtectionAmulets > 0)
                    Console.WriteLine("  [4] 아이템 사용");

                Console.WriteLine("  [5] 상점");
                Console.WriteLine("  [Q] 종료");
            }

            Console.Write("  > ");
        }

        /// <summary>
        /// 키 입력을 받아 해당 Command를 디스패치한다.
        /// 반환값이 false이면 게임 루프를 종료한다.
        /// </summary>
        public bool HandleInput(ConsoleKey key)
        {
            var state = _engine.State;

            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new EnhanceCommand());
                    else
                        Console.WriteLine(InvalidInputMessage);
                    return true;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new SellCommand());
                    else
                        Console.WriteLine(InvalidInputMessage);
                    return true;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    // 파괴대기 상태: 파괴 확정, 그 외: 도감 수집
                    if (state.PendingAdProtection)
                        DispatchAndSave(new ConfirmDestroyCommand());
                    else
                        DispatchAndSave(new CollectCommand());
                    return true;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new UseItemCommand());
                    else
                        Console.WriteLine(InvalidInputMessage);
                    return true;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    if (!state.PendingAdProtection)
                        EnterShop();
                    else
                        Console.WriteLine(InvalidInputMessage);
                    return true;

                case ConsoleKey.Q:
                    Console.WriteLine("게임을 종료합니다.");
                    return false;

                default:
                    Console.WriteLine(InvalidInputMessage);
                    return true;
            }
        }

        /// <summary>
        /// 상점 서브메뉴. 파편으로 아이템을 교환하며, [0]으로 메인 메뉴에 복귀한다.
        /// 비용은 ExchangeCommand의 상수를 참조하여 표시/로직 간 불일치를 방지한다.
        /// </summary>
        private void EnterShop()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"  === 상점 === [파편: {_engine.State.PlayerData.Fragments}개]");
                Console.WriteLine($"  [1] 보호부적 구매 ({ExchangeCommand.ProtectionAmuletCost} 파편)");
                Console.WriteLine($"  [2] 골드 주머니 구매 ({ExchangeCommand.GoldPouchCost} 파편 → {ExchangeCommand.GoldPouchReward}G)");
                Console.WriteLine("  [0] 돌아가기");
                Console.Write("  > ");

                var key = Console.ReadKey(true).Key;
                Console.WriteLine();

                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        DispatchAndSave(new ExchangeCommand(ItemTypes.ProtectionAmulet));
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        DispatchAndSave(new ExchangeCommand(ItemTypes.GoldPouch));
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        return;

                    default:
                        Console.WriteLine(InvalidInputMessage);
                        break;
                }
            }
        }
    }
}
