using CuiApp.Providers;
using CuiApp.Rendering;
using CuiApp.Repositories;
using CuiApp.Screens;
using GameCore.Data;
using GameCore.Engine;
using GameCore.Logic;

// --- CLI options parsing ---
int? seed = null;
bool useAscii = false;
bool useColor = true;
string? savePath = null;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--seed" when i + 1 < args.Length:
            seed = int.Parse(args[++i]);
            break;
        case "--ascii":
            useAscii = true;
            break;
        case "--no-color":
            useColor = false;
            break;
        case "--save" when i + 1 < args.Length:
            savePath = args[++i];
            break;
    }
}

// --- Initialization ---
var random = new ConsoleRandomProvider(seed);
var time = new ConsoleTimeProvider();
var repo = new JsonFileRepository(savePath);
// CSV는 실행 파일과 같은 디렉토리에서 찾기
var baseDir = AppContext.BaseDirectory;
var swordTable = SwordDataLoader.Load(Path.Combine(baseDir, "swords.csv"));
var context = new GameContext(random, time, swordTable, isAdSystemEnabled: false);

var playerData = repo.Load();
var initialState = GameSessionLogic.CreateInitialState(playerData, swordTable);
var engine = new GameEngine(initialState, context);

var renderer = new ConsoleRenderer(useAscii, useColor);
var eventRenderer = new EventRenderer(renderer);
var screens = new ScreenManager();

screens.Register("title", new TitleScreen(renderer));
screens.Register("enhance", new EnhanceScreen(renderer));
screens.Register("workshop", new WorkshopScreen(renderer));

// Event queue
engine.OnEvent += evt => eventRenderer.Enqueue(evt);

// --- Game Loop ---
screens.SwitchTo("title");

while (true)
{
    renderer.Clear();
    screens.Render(engine.State, context);

    char key;
    if (Console.IsInputRedirected)
    {
        var c = Console.Read();
        if (c == -1) break;  // EOF
        key = (char)c;
    }
    else
    {
        key = Console.ReadKey(intercept: true).KeyChar;
    }

    if ((key == 'q' || key == 'Q') && screens.CurrentName != "title")
        break;

    screens.HandleInput(key, engine, context);
    eventRenderer.PlayAll();

    if (eventRenderer.ShouldSave)
        repo.Save(engine.State.PlayerData);
}

// Final save
repo.Save(engine.State.PlayerData);

renderer.Clear();
renderer.PrintCenter("저장 완료. 안녕히!", ConsoleColor.Cyan);
try { Console.CursorVisible = true; } catch (IOException) { }
Console.WriteLine();
