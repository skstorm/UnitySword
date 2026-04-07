using System;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Models;

namespace ConsoleApp
{
    public class MenuHandler
    {
        private readonly GameEngine _engine;
        private readonly SwordDataTable _swordTable;
        private readonly SaveManager _saveManager;

        public MenuHandler(GameEngine engine, SwordDataTable swordTable, SaveManager saveManager)
        {
            _engine = engine;
            _swordTable = swordTable;
            _saveManager = saveManager;
        }

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

        public bool HandleInput(ConsoleKey key)
        {
            var state = _engine.State;

            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (!state.PendingAdProtection)
                    {
                        _engine.Dispatch(new EnhanceCommand());
                        _saveManager.Save(_engine.State.PlayerData);
                    }
                    else
                    {
                        Console.WriteLine("  잘못된 입력입니다.");
                    }
                    return true;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (!state.PendingAdProtection)
                    {
                        _engine.Dispatch(new SellCommand());
                        _saveManager.Save(_engine.State.PlayerData);
                    }
                    else
                    {
                        Console.WriteLine("  잘못된 입력입니다.");
                    }
                    return true;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    if (state.PendingAdProtection)
                    {
                        _engine.Dispatch(new ConfirmDestroyCommand());
                        _saveManager.Save(_engine.State.PlayerData);
                    }
                    else
                    {
                        _engine.Dispatch(new CollectCommand());
                        _saveManager.Save(_engine.State.PlayerData);
                    }
                    return true;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    if (!state.PendingAdProtection)
                    {
                        _engine.Dispatch(new UseItemCommand());
                        _saveManager.Save(_engine.State.PlayerData);
                    }
                    else
                    {
                        Console.WriteLine("  잘못된 입력입니다.");
                    }
                    return true;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    if (!state.PendingAdProtection)
                    {
                        EnterShop();
                    }
                    else
                    {
                        Console.WriteLine("  잘못된 입력입니다.");
                    }
                    return true;

                case ConsoleKey.Q:
                    Console.WriteLine("게임을 종료합니다.");
                    return false;

                default:
                    Console.WriteLine("  잘못된 입력입니다.");
                    return true;
            }
        }

        private void EnterShop()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"  === 상점 === [파편: {_engine.State.PlayerData.Fragments}개]");
                Console.WriteLine("  [1] 보호부적 구매 (30 파편)");
                Console.WriteLine("  [2] 골드 주머니 구매 (10 파편 → 500G)");
                Console.WriteLine("  [0] 돌아가기");
                Console.Write("  > ");

                var key = Console.ReadKey(true).Key;
                Console.WriteLine();

                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        _engine.Dispatch(new ExchangeCommand(ItemTypes.ProtectionAmulet));
                        _saveManager.Save(_engine.State.PlayerData);
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        _engine.Dispatch(new ExchangeCommand(ItemTypes.GoldPouch));
                        _saveManager.Save(_engine.State.PlayerData);
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        return;

                    default:
                        Console.WriteLine("  잘못된 입력입니다.");
                        break;
                }
            }
        }
    }
}
