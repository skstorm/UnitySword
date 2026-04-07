using GameCore.Util;

namespace CuiApp.Providers;

public class ConsoleTimeProvider : ITimeProvider
{
    public DateTime Now() => DateTime.Now;
}
