using System;
using System.IO;
using GameCore.Commands;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Models;
using GameCore.Util;
using Spectre.Console;
using GameApp;

namespace TerminalUI
{
    /// <summary>
    /// TerminalUI 진입점. CSV 데이터 로드 → 세이브 로드 → GameEngine 생성 → GameScreen 시작.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1. CSV 데이터 로드
            var csvContent = LoadCsv("swords.csv",
                Path.Combine("Project", "Assets", "Resources", "Data", "swords.csv"));
            var swords = new SwordDataLoader().Parse(csvContent);
            var swordTable = new SwordDataTable(swords);

            // 2. 마스터리 데이터 로드
            MasteryDataTable masteryTable = null;
            var masteryCsv = LoadCsvOptional("mastery_levels.csv",
                Path.Combine("doc", "mastery_levels.csv"));
            if (masteryCsv != null)
            {
                var masteryLevels = new MasteryDataLoader().Parse(masteryCsv);
                masteryTable = new MasteryDataTable(masteryLevels);
            }

            // 3. 세이브 로드
            var saveManager = new SaveManager();
            var playerData = saveManager.Load() ?? new PlayerData();
            if (saveManager.LastLoadCorrupted)
                AnsiConsole.MarkupLine("[red]  [!] 저장 파일이 손상되어 새로 시작합니다.[/]");

            // 4. GameEngine 생성
            var random = new SeededRandomProvider(Environment.TickCount);
            var time = new FakeTimeProvider();
            var context = new GameContext(random, time, swordTable, masteryTable);
            var engine = GameEngine.Create(playerData, context);

            // 5. GameScreen 시작
            var screen = new GameScreen(engine, swordTable, saveManager);
            screen.Run();
        }

        /// <summary>필수 CSV 로드. 여러 경로에서 순서대로 탐색한다.</summary>
        static string LoadCsv(string fileName, string fallbackRelative)
        {
            var paths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", fallbackRelative)
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }

            throw new FileNotFoundException(
                $"{fileName} not found. Searched: {string.Join(", ", paths)}");
        }

        /// <summary>선택적 CSV 로드. 파일이 없으면 null 반환.</summary>
        static string LoadCsvOptional(string fileName, string fallbackRelative)
        {
            try { return LoadCsv(fileName, fallbackRelative); }
            catch (FileNotFoundException) { return null; }
        }
    }
}
