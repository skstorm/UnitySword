using System;
using System.Threading;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Events;
using GameCore.Models;
using Spectre.Console;

namespace TerminalUI
{
    /// <summary>
    /// Live Display 메인 루프. 3단 레이아웃(리소스/게임/커맨드)을 조립하고,
    /// 키 입력을 받아 GameEngine에 Command를 디스패치한다.
    /// </summary>
    public class GameScreen
    {
        private readonly GameEngine _engine;
        private readonly SwordDataTable _swordTable;
        private readonly SaveManager _saveManager;
        private readonly SwordView _swordView;
        private bool _inShop;
        private bool _running = true;

        public GameScreen(GameEngine engine, SwordDataTable swordTable, SaveManager saveManager)
        {
            _engine = engine;
            _swordTable = swordTable;
            _saveManager = saveManager;
            _swordView = new SwordView(swordTable);

            // 이벤트 구독: 최근 이벤트를 SwordView에 전달
            _engine.OnEvent += HandleEvent;
        }

        private void HandleEvent(GameEvent evt)
        {
            var formatted = EventFormatter.Format(evt);
            if (formatted != null)
                _swordView.SetLastEvent(formatted);
        }

        /// <summary>Command를 디스패치하고 변경된 PlayerData를 즉시 저장한다.</summary>
        private void DispatchAndSave(Command cmd)
        {
            _engine.Dispatch(cmd);
            _saveManager.Save(_engine.State.PlayerData);
        }

        /// <summary>Live Display 게임 루프를 시작한다.</summary>
        public void Run()
        {
            AnsiConsole.Clear();

            while (_running)
            {
                // 화면 렌더
                AnsiConsole.Clear();
                RenderScreen();

                // 키 입력 대기
                var key = Console.ReadKey(true).Key;

                if (_inShop)
                    HandleShopInput(key);
                else
                    HandleMainInput(key);
            }

            AnsiConsole.MarkupLine("\n[grey]게임을 종료합니다.[/]");
        }

        private void RenderScreen()
        {
            var state = _engine.State;

            // 상단: 리소스 바
            AnsiConsole.Write(ResourceBar.Render(state));

            // 중앙: 게임 영역 또는 상점
            if (_inShop)
                AnsiConsole.Write(ShopView.Render(state));
            else
                AnsiConsole.Write(_swordView.Render(state));

            // 하단: 커맨드 바
            if (_inShop)
                AnsiConsole.Write(CommandBar.RenderShop());
            else
                AnsiConsole.Write(CommandBar.RenderMain(state, _swordTable));
        }

        private void HandleMainInput(ConsoleKey key)
        {
            var state = _engine.State;

            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new EnhanceCommand());
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new SellCommand());
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    if (state.PendingAdProtection)
                        DispatchAndSave(new ConfirmDestroyCommand());
                    else
                        DispatchAndSave(new CollectCommand());
                    break;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    if (!state.PendingAdProtection)
                        DispatchAndSave(new UseItemCommand());
                    break;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    if (!state.PendingAdProtection)
                        _inShop = true;
                    break;

                case ConsoleKey.Q:
                    _running = false;
                    break;
            }
        }

        private void HandleShopInput(ConsoleKey key)
        {
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
                    _inShop = false;
                    _swordView.SetLastEvent(null);
                    break;
            }
        }
    }
}
