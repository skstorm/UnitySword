namespace CuiApp.Rendering;

public static class SwordAsciiArt
{
    public static string[] Get(int level) => level switch
    {
        0 => WoodenSword,
        <= 5 => BasicSword,
        <= 9 => MagicSword,
        <= 14 => LegendarySword,
        <= 17 => MythicSword,
        _ => CosmicSword,
    };

    private static readonly string[] WoodenSword =
    [
        "      |      ",
        "      |      ",
        "     ===     ",
        "      |      ",
    ];

    private static readonly string[] BasicSword =
    [
        "      /\\      ",
        "     /  \\     ",
        "     /  \\     ",
        "     \\  /     ",
        "      ||      ",
        "     ====     ",
        "      ||      ",
    ];

    private static readonly string[] MagicSword =
    [
        "    * /\\ *    ",
        "     /  \\     ",
        "    */  \\*    ",
        "     /  \\     ",
        "     \\  /     ",
        "    +=||=+    ",
        "      ||      ",
    ];

    private static readonly string[] LegendarySword =
    [
        "   \\  /\\  /   ",
        "    \\/  \\/    ",
        "    /\\  /\\    ",
        "   /  \\/  \\   ",
        "   \\  /\\  /   ",
        "    \\ || /    ",
        "   ==+||+==   ",
        "     ||||     ",
    ];

    private static readonly string[] MythicSword =
    [
        "  ~ \\  /\\ / ~ ",
        "   * \\/  \\/ *  ",
        "    */\\  /\\*   ",
        "   / *\\/  \\    ",
        "   \\  /\\* /    ",
        "  ~ \\ || / ~   ",
        "   ==+||+==    ",
        "     |##|      ",
    ];

    private static readonly string[] CosmicSword =
    [
        " *  . \\  /\\  / .  * ",
        "  .  * \\/  \\/ *  .  ",
        " *   */\\  /\\*   *  ",
        "  . / *\\/  \\ .     ",
        " *  \\  /\\* /  *    ",
        "  . ~\\ || /~ .     ",
        "   ==+|##|+==      ",
        "  *  |####|  *     ",
        "     |####|        ",
    ];
}
