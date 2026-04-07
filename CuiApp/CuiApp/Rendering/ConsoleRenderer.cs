namespace CuiApp.Rendering;

public class ConsoleRenderer
{
    public int Width { get; }
    public bool UseAscii { get; }
    public bool UseColor { get; }

    public ConsoleRenderer(bool useAscii = false, bool useColor = true)
    {
        UseAscii = useAscii;
        UseColor = useColor;

        try { Width = Math.Max(Console.WindowWidth, 36); }
        catch { Width = 40; }
    }

    public void Clear()
    {
        Console.Clear();
        Console.CursorVisible = false;
    }

    public void Flash(ConsoleColor color)
    {
        if (!UseColor) return;
        var prev = Console.BackgroundColor;
        Console.BackgroundColor = color;
        Thread.Sleep(80);
        Console.BackgroundColor = prev;
    }

    public void PrintCenter(string text, ConsoleColor? color = null)
    {
        if (color.HasValue && UseColor)
            Console.ForegroundColor = color.Value;

        var pad = Math.Max(0, (Width - GetDisplayWidth(text)) / 2);
        Console.WriteLine(new string(' ', pad) + text);

        if (color.HasValue && UseColor)
            Console.ResetColor();
    }

    public void Print(string text, ConsoleColor? color = null)
    {
        if (color.HasValue && UseColor)
            Console.ForegroundColor = color.Value;

        Console.Write(text);

        if (color.HasValue && UseColor)
            Console.ResetColor();
    }

    public void PrintLine(string text = "", ConsoleColor? color = null)
    {
        if (color.HasValue && UseColor)
            Console.ForegroundColor = color.Value;

        Console.WriteLine(text);

        if (color.HasValue && UseColor)
            Console.ResetColor();
    }

    public void Pause(int ms) => Thread.Sleep(ms);

    public void WaitKey()
    {
        PrintCenter("[아무 키] 계속", ConsoleColor.DarkGray);
        Console.ReadKey(intercept: true);
    }

    public void DrawBox(string[] lines)
    {
        var inner = Width - 4;
        PrintLine("┌" + new string('─', inner + 2) + "┐");
        foreach (var line in lines)
        {
            var trimmed = line.Length > inner ? line[..inner] : line;
            var padding = inner - GetDisplayWidth(trimmed);
            PrintLine("│ " + trimmed + new string(' ', Math.Max(0, padding)) + " │");
        }
        PrintLine("└" + new string('─', inner + 2) + "┘");
    }

    public void DrawSeparator()
    {
        var inner = Width - 4;
        PrintLine("├" + new string('─', inner + 2) + "┤");
    }

    public string ProgressBar(int current, int max, int width = 20)
    {
        if (max <= 0) return new string('░', width);
        var filled = (int)((double)current / max * width);
        filled = Math.Clamp(filled, 0, width);
        return new string('█', filled) + new string('░', width - filled);
    }

    public string FormatGold(int gold)
    {
        var symbol = UseAscii ? "$" : "💰";
        return $"{symbol} {gold:N0} G";
    }

    private static int GetDisplayWidth(string text)
    {
        // Rough estimate: CJK and emoji count as 2, others as 1
        int width = 0;
        foreach (var c in text)
        {
            if (c >= 0x1100 && c <= 0x115F) width += 2;      // Hangul Jamo
            else if (c >= 0x2E80 && c <= 0x9FFF) width += 2;  // CJK
            else if (c >= 0xAC00 && c <= 0xD7AF) width += 2;  // Hangul Syllables
            else if (c >= 0xF900 && c <= 0xFAFF) width += 2;  // CJK Compatibility
            else width += 1;
        }
        return width;
    }
}
